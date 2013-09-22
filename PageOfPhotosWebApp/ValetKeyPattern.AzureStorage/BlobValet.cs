using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace ValetKeyPattern.AzureStorage
{
   public class BlobValet : AzureStorageValet
   {
      /// <summary>
      /// "a block blob can include no more than 50,000 blocks" ref: http://msdn.microsoft.com/en-us/library/windowsazure/ee691964.aspx 
      /// Let's assume they are sequentially numbered, starting with 0
      /// </summary>
      internal int MaxBlockBlobNumber
      {
         get { return 49999; }
      }

      /// <summary>
      /// Really a block "name" since it is not necessarily a number (and Azure REST calls it a name)
      /// </summary>
      /// <param name="blockBlobNumber"></param>
      /// <param name="maxWidth"></param>
      /// <returns></returns>
      internal string FormatBlockBlobNumber(int blockBlobNumber, int maxWidth = 5)
      {
         maxWidth = 5; // <<== NORMALIZE THIS WHEN WE BASE64-encode, else APPENDS WILL BE TOO LARGE

         // convention is to use Base64-encode the block numbers since they all are STRINGS and (within a blob) ALL must be SAME LENGTH
         // var blockId = Convert.ToBase64String(Encoding.Default.GetBytes(blockList.Count().ToString()));
         Contract.Assert(blockBlobNumber <= MaxBlockBlobNumber);
         var numberFormat = String.Format("D{0}", maxWidth);
         var blockId = blockBlobNumber.ToString(numberFormat); // default is 00000..49000
#if false
         blockId = "X" + blockId;
         blockId = "N01";
#else // TODO: figure out on the fly whether the names given to these segments are Base64-encoded?
         // blockId = blockBlobNumber.ToString();
         var bytes = Encoding.UTF8.GetBytes(blockId);
         blockId = Convert.ToBase64String(bytes);
#endif
         return blockId;
      }

      internal Stream CreateStreamFromString(string text)
      {
         //var stream = new MemoryStream(UTF8Encoding.Default.GetBytes(text));
         //var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
         var stream = new MemoryStream(Encoding.ASCII.GetBytes(text));
         return stream;
      }

      public string ContainerName
      {
         get { return ValetKeyUri.StoragePartitionName(); }
      }

      public BlobValet(string url)
         : base(url)
      {
      }

      public BlobValet(Uri uri)
         : base(uri)
      {
      }

#if false
      public QueueValet CreateQueueValetUri(string storageAccountName, string queueName, string sasQueryString,
                                               string urlScheme = UrlSchemeDefault, bool autorepair = AutoRepairDefault)
      {
         // TODO: this will FAIL for local dev environment
         var uri = new Uri(String.Format("{0}://{1}.queue.core.windows.net/{2}{3}", urlScheme, storageAccountName, queueName, sasQueryString));
         return new QueueValet(uri);
      }
#endif

      internal string GetContainerName(Uri valetKeyUri)
      {
         Contract.Assert(valetKeyUri.Segments.Length > 1);
         var lastSegment = valetKeyUri.Segments.Length - 1;
         var containerName = valetKeyUri.Segments[lastSegment]; // /foo/bar = "/" + "foo/" + "bar"
         return containerName;
      }

#if false // don't think it is possible to CREATE a container (since the SAS will be for that non-existant container or a blob within it)
      /// THIS IS A BAD IDEA
      ///            // TODO: this is a crime - place it somewhere more appropriate           
           // Make sure my storage is ready
           var blobValetKeyUrl = ConfigurationManager.AppSettings["MediaStorageValetKeyUrl"];
           var blobValet = new BlobValet(blobValetKeyUrl);
           blobValet.EnsureBlobContainerExists(true);

      public void EnsureBlobContainerExists(bool publicRead = false)
      {
         var containerName = GetContainerName(ValetKeyUri);
         var destinationUri = new Uri(GetDestinationPath(ValetKeyUri).AbsoluteUri + "/foo");
         var cloudBlob = new CloudBlockBlob(destinationUri, StorageCredentials);

         var blobClient = new CloudBlobClient(destinationUri, StorageCredentials);
         var container = blobClient.GetContainerReference(containerName);

         var newlyCreated = container.CreateIfNotExists();

         if (newlyCreated && publicRead)
         {
            var containerPermissions = new BlobContainerPermissions()
            {
               PublicAccess = BlobContainerPublicAccessType.Container
            };
            container.SetPermissions(containerPermissions);
         }
      }
#endif

      /// <summary>
      /// Clean up stream object.
      /// </summary>
      /// <param name="result"></param>
      private void BlockBlobUploadCompleteAndSetMimeType(IAsyncResult result)
      {
         try
         {
            var blockBlob = (CloudBlockBlob) result.AsyncState;

            string mimeType = GetSupportedMimeTypeFromFileName(blockBlob.Name);
            if (mimeType != UnknownMimeType)
            {
               // PoP does not want to upload files that are not supported - though we should usually not get this far in the processing

               blockBlob.EndUploadFromStream(result);

               blockBlob.Properties.ContentType = mimeType;
               blockBlob.Properties.CacheControl = "";
               blockBlob.BeginSetProperties(ar => (ar.AsyncState as CloudBlockBlob).EndSetProperties(ar), blockBlob);
            }
            else
            {
               throw new ArgumentException("Don't know how to handle mime type of " + mimeType + " within IAsyncResult for blob to be named " + blockBlob.Name, "result");
            }
         }
         catch (Exception ex)
         {
            Trace.TraceError("Exception when mopping up a BlockBlobUploadCompleteAndSetMimeType sorta thing - blob name => {0} - ex [{1}] / [{2}]", ((CloudBlockBlob)result.AsyncState).Name,
               ex.GetBaseException(), ex);
            // TODO: WHAT ELSE YOU GOT?
            throw;
         }
      }

      const string UnknownMimeType = "application/octet-stream"; // "application/unknown"
      // For reals might want to allow different users to have Claims which allow different mime types - for example, certain users are allowed video
      public string GetSupportedMimeTypeFromFileName(string filename)
      {
         var mimetype = System.Web.MimeMapping.GetMimeMapping(filename);
         switch (mimetype)
         {
            case "image/png":
            case "image/jpeg":
               return mimetype;
            default:
               return UnknownMimeType;
         }
      }

      protected override Uri GetSpecificBaseUri()
      {
         return CloudStorageAccount.DevelopmentStorageAccount.CreateCloudBlobClient().BaseUri;
      }

      /// <summary>
      /// Full blob Uri is figured out using the storage account and container already known to the BlobValet, then appending blobFileName
      /// </summary>
      /// <param name="blobFileName"></param>
      /// <param name="stream"></param>
      /// <param name="mimeType"></param>
      /// <param name="byteCount">Count of bytes in the stream. Not used at this time. May be used in future to optimize the upload to blob storage, for telemetry, or to block uploads over a certain size.</param>
      public void UploadStreamToBlob(string blobFileName, Stream stream, string mimeType = null, int? byteCount = null)
      {
         var destinationUri = new Uri(String.Format("https://{0}.blob.code.windows.net/{1}/{2}", StorageCredentials.AccountName, ContainerName, blobFileName));
         UploadStream(destinationUri, stream, mimeType, byteCount);
      }

      /// <summary>
      /// </summary>
      /// <param name="destinationUri"></param>
      /// <param name="stream"></param>
      /// <param name="mimeType"></param>
      /// <param name="byteCount">Count of bytes in the stream. Not used at this time. May be used in future to optimize the upload to blob storage, for telemetry, or to block uploads over a certain size.</param>
      public void UploadStream(Uri destinationUri, Stream stream, string mimeType = null, int? byteCount = null)
      {
         var cloudBlob = new CloudBlockBlob(destinationUri, StorageCredentials);

         try
         {
#if false
            cloudBlob.BeginUploadFromStream(stream,
               AccessCondition.GenerateEmptyCondition(),
               new BlobRequestOptions
               {
                  RetryPolicy =
                     new ExponentialRetry(TimeSpan.FromSeconds(3), 5)
               },
               null,
               BlockBlobUploadCompleteAndSetMimeType,
               cloudBlob); // TODO: change to struct that passes in both blob and mimetype
#else
            cloudBlob.UploadFromStream(stream,
               AccessCondition.GenerateEmptyCondition(),
               new BlobRequestOptions
               {
                  RetryPolicy =
                     new ExponentialRetry(TimeSpan.FromSeconds(3), 5)
               },
               null);

            cloudBlob.Properties.ContentType = mimeType;
            cloudBlob.Properties.CacheControl = "";
            cloudBlob.BeginSetProperties(ar => (ar.AsyncState as CloudBlockBlob).EndSetProperties(ar), cloudBlob);
#endif
         }
         catch (StorageException ex)
         {
            // TODO: dude - how about some error handling?
            Trace.TraceError(ex.ToString());
            throw;
         }
      }

#if true // UploadFile, with unimplemented BuildDestinationUri

      /// <summary>
      /// Given a Valet Key which allows writing/creating blobs for a Blob Container,
      /// write the given file into the equivalently-named blob.
      /// Currently ignores situations where the source filename is not allowed as the blob name.
      /// Name in blob storage is based on sourceFilePath and is placed in the Container referenced
      /// by valetKeyUrl.
      /// </summary>
      public void UploadFile(string sourceFilePath)
      {
         var desinationUri = BuildDestinationUri(ValetKeyUri.AbsoluteUri, sourceFilePath);

         string acceptHeaderValue = null;
         if (sourceFilePath.Contains("https://api.github.com"))
         {
            // horrible, horrible hack for the github webhook handler
            acceptHeaderValue = "application/vnd.github.raw"; // TODO: this path has no specific unit tests
         }

         // TODO: if already exists, preserve Content Type (mime-type) and Cache Control values ()
         // TODO: ... and probably Content Encoding (e.g., "gzip") and Content Language (e.g., "da" or "de, en") values
         // TODO: consider setting mime-type, caching, other headers on first version
         // TODO: could allow snapshotting when not first version
         // TODO: could become async for scale and reliability
         // TODO: consider destinationBlob.StartCopyFromBlob(sourceFilePath) for async copy
         // TODO: also support files (vs HTTP only): using (var stream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
         using (var httpClient = new HttpClient())
         {
            if (!String.IsNullOrEmpty(acceptHeaderValue))
            {
               httpClient.DefaultRequestHeaders.Add("Accept", acceptHeaderValue);
            }
            var stream = httpClient.GetStreamAsync(sourceFilePath).Result;
            UploadStream(desinationUri, stream);
         }
      }
#endif

      /// <summary>
      /// REFERENCE: http://stackoverflow.com/questions/14652172/azure-blobs-block-list-is-empty-but-blob-is-not-empty-how-can-this-be
      /// Facts: 
      /// 1. Put Block List approach does NOT work in Storage Emulator (!)
      /// 2. A Block Blob that was populated using Put Block List can be appended by putting more Blocks
      /// 3. A Block Blob that was populated using a Put Blob (single stream) cannot be mixed with Blocks, so must be rewritten to append to it
      /// Consequences - each of the above facts introduces a new special case
      /// 
      /// REFERENCE: http://msdn.microsoft.com/en-us/library/windowsazure/ee691964.aspx
      /// (QUOTING)
      /// "
      /// You can modify an existing block blob by inserting, replacing, or deleting existing blocks. After uploading the block or blocks that 
      /// have changed, you can commit a new version of the blob by committing the new blocks with the existing blocks you want to keep using a 
      /// single commit operation. To insert the same range of bytes in two different locations of the committed blob, you can commit the same 
      /// block in two places within the same commit operation. For any commit operation, if any block is not found, the entire commitment 
      /// operation fails with an error, and the blob is not modified. Any block commitment overwrites the blob’s existing properties and 
      /// metadata, and discards all uncommitted blocks.
      ///
      /// Block IDs are strings of equal length within a blob. Block client code usually uses base-64 encoding to normalize strings into equal 
      /// lengths. When using base-64 encoding, the pre-encoded string must be 64 bytes or less. Block ID values can be duplicated in different 
      /// blobs. A blob can have up to 100,000 uncommitted blocks, but their total size cannot exceed 400 GB.
      /// 
      /// If you write a block for a blob that does not exist, a new block blob is created, with a length of zero bytes. This blob will appear 
      /// in blob lists that include uncommitted blobs. If you don’t commit any block to this blob, it and its uncommitted blocks will be 
      /// discarded one week after the last successful block upload. All uncommitted blocks are also discarded when a new blob of the same name 
      /// is created using a single step (rather than the two-step block upload-then-commit process).
      /// "
      /// </summary>
      /// <param name="destinationUri"></param>
      /// <param name="text"></param>
      public void AppendToBlob(Uri destinationUri, string text)
      {
         var cloudBlob = new CloudBlockBlob(destinationUri, StorageCredentials);

         var stream = CreateStreamFromString(text);
         try
         {
            try
            {
               var existingBlockItems = cloudBlob.DownloadBlockList(BlockListingFilter.Committed).ToList();
               var existingBlockCount = existingBlockItems.Count();
               if (existingBlockCount > 0)
               {
                  AppendToExistingBlockBlobUsingBlocks(destinationUri, text, existingBlockItems);
               }
               else
               {
                  AppendToExistingBlockBlobUsingStream(destinationUri, text);
               }
            }
            catch (StorageException storageEx)
            {
               if (storageEx.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Forbidden)
               {
                  // perhaps the SAS is malformed?
                  throw;
               }
               if (storageEx.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
               // == HttpStatusCode.NotFound)  ///////////StorageErrorCodeStrings.ResourceNotFound)
               {
                  // this is normal - the blob will be created for us

                  //// cloudBlob.Container.CreateIfNotExists(); // DO WE NEED TO DO THIS? or OUGHT we?

#if false
                  if (IsEmulated())
                  {
                     CreateNewBlockBlobUsingStream(destinationUri, text);
                  }
                  else
#endif
                  {
                     CreateNewBlockBlobUsingBlocks(destinationUri, text);
                  }
               }
               else
               {
                  throw;
               }
            }

            cloudBlob.Properties.ContentType = "text/plain";
            cloudBlob.Properties.CacheControl = "max-age: 60";
            cloudBlob.BeginSetProperties(ar => (ar.AsyncState as CloudBlockBlob).EndSetProperties(ar), cloudBlob);
         }
         catch (StorageException ex)
         {
            // TODO: dude - how about some error handling?
            Trace.TraceError(ex.ToString());
            throw;
         }
      }

      internal void CreateNewBlockBlobUsingStream(Uri destinationUri, string text)
      {
         var cloudBlob = new CloudBlockBlob(destinationUri, StorageCredentials);
         cloudBlob.UploadFromStream(CreateStreamFromString(text));
      }

      internal void AppendToExistingBlockBlobUsingStream(Uri destinationUri, string text)
      {
         var cloudBlob = new CloudBlockBlob(destinationUri, StorageCredentials);
         var oldTextStream = new MemoryStream();
         cloudBlob.DownloadToStream(oldTextStream);
         oldTextStream.Position = 0;
         var streamReader = new StreamReader(oldTextStream);
         var oldText = streamReader.ReadToEnd();
         var combinedText = oldText + text;
         cloudBlob.UploadFromStream(CreateStreamFromString(combinedText));

      }

      internal void CreateNewBlockBlobUsingBlocks(Uri destinationUri, string text)
      {
         AppendToBlockBlobUsingBlocks(destinationUri, text);
      }

      internal void AppendToExistingBlockBlobUsingBlocks(Uri destinationUri, string text,
         List<ListBlockItem> exstingBlockItems)
      {
         Contract.Requires(exstingBlockItems != null);
         var existingBlockCount = exstingBlockItems.Count();
         Contract.Assert(existingBlockCount > 0);
         AppendToBlockBlobUsingBlocks(destinationUri, text, exstingBlockItems);
      }

      private void AppendToBlockBlobUsingBlocks(Uri destinationUri, string text,
         List<ListBlockItem> existingBlockItems = null)
      {
         Contract.Requires(!IsEmulated()); // not supported within Cloud Storage Emulator
         var cloudBlob = new CloudBlockBlob(destinationUri, StorageCredentials);

         var completedStreamUploads = 0;
         var updatedBlockList = new List<string>();

         var nextBlockNumber = 0;
         var blockId1 = FormatBlockBlobNumber(1);
         var blockId40000 = FormatBlockBlobNumber(40000);

         var blockId = FormatBlockBlobNumber(nextBlockNumber);
         var previousBlockCount = 0;

         if (existingBlockItems != null)
         {
            previousBlockCount = existingBlockItems.Count();
            nextBlockNumber = previousBlockCount;
            blockId = FormatBlockBlobNumber(nextBlockNumber, existingBlockItems[0].Name.Length);
            updatedBlockList.AddRange(existingBlockItems.Select(bi => bi.Name));
         }

         var stream = CreateStreamFromString(text);
         {
#if false
              cloudBlob.BeginPutBlock(blockId, stream, null, AccessCondition.GenerateEmptyCondition(), null, null, ar =>
                                                      {
                                                          (ar.AsyncState as CloudBlockBlob).EndUploadFromStream(ar);
                                                          completedStreamUploads++;
                                                      }, cloudBlob);
#else
            //var cond = AccessCondition.GenerateEmptyCondition();
            cloudBlob.PutBlock(blockId, stream, null);
            completedStreamUploads++;
#endif
            updatedBlockList.Add(blockId);

            Console.WriteLine("Waiting ... {0} . {1}", completedStreamUploads, updatedBlockList.Count);
            while (previousBlockCount + completedStreamUploads < updatedBlockList.Count)
            {
               Console.WriteLine("Waiting ... {0} < {1}", completedStreamUploads, updatedBlockList.Count);
            }
            cloudBlob.PutBlockList(updatedBlockList);

            cloudBlob.Properties.ContentType = "text/plain";
            cloudBlob.Properties.CacheControl = "max-age: 60";
            cloudBlob.BeginSetProperties(ar => (ar.AsyncState as CloudBlockBlob).EndSetProperties(ar), cloudBlob);
         }

         // string blockIdBase64 = Convert.ToBase64String(System.BitConverter.GetBytes(id));
         // blob.PutBlock(blockIdBase64, new MemoryStream(buffer, true), null); 
         // blocklist.Add(blockIdBase64);
         // id++;
         // string blockId = Convert.ToBase64String(System.BitConverter.GetBytes(id));
         // blob.PutBlock(blockId, new MemoryStream(finalbuffer, true), null);
         // blocklist.Add(blockId);
         // blob.PutBlockList(blocklist); 
      }
   }
}
