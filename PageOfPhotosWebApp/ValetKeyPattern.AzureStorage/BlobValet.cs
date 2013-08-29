using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace ValetKeyPattern.AzureStorage
{
   public class BlobValet : AzureStorageValet
   {
      public string ContainerName { get { return ValetKeyUri.StoragePartitionName(); } }

      public BlobValet(string url) : base(url)
      {
         
      }

      public BlobValet(Uri uri) : base(uri)
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


      /// <summary>
      /// Upload the blob and then (if nothing went wrong) drop a message on the queue announcing the blob
      /// </summary>
      /// <param name="stream">Might be from File Upload via web page</param>
      /// <param name="origFilename"></param>
      public void UploadStreamToBlob(Stream stream, string origFilename, string origMimeType, int byteCount)
      {
      }

      /// <summary>
      /// Clean up stream object.
      /// </summary>
      /// <param name="result"></param>
      private void BlockBlobUploadComplete(IAsyncResult result)
      {
         var blockBlob = (CloudBlockBlob)result.AsyncState;
         blockBlob.EndUploadFromStream(result);
      }

      /// <summary>
      /// Infer from Uri version
      /// </summary>
      /// <param name="destinationUrl"></param>
      /// <param name="stream"></param>
      /// <param name="mimeType"></param>
      /// <param name="byteCount">Count of bytes in the stream. Not used at this time. May be used in future to optimize the upload to blob storage, for telemetry, or to block uploads over a certain size.</param>
      public void UploadStream(string destinationUrl, Stream stream, string mimeType = null, int? byteCount = null)
      {
         UploadStream(new Uri(destinationUrl), stream, mimeType, byteCount);
      }

      /// <summary>
      /// </summary>
      /// <param name="destinationUri"></param>
      /// <param name="stream"></param>
      /// <param name="mimeType"></param>
      /// <param name="byteCount">Count of bytes in the stream. Not used at this time. May be used in future to optimize the upload to blob storage, for telemetry, or to block uploads over a certain size.</param>
      public void UploadStream(Uri destinationUri, Stream stream, string mimeType = null, int? byteCount = null)
      {
         var credentials = new StorageCredentials(SasToken);
         var cloudBlob = new CloudBlockBlob(destinationUri, credentials);

         try
         {
            cloudBlob.BeginUploadFromStream(stream,
               AccessCondition.GenerateEmptyCondition(),
               new BlobRequestOptions
               {
                  RetryPolicy =
                     new ExponentialRetry(TimeSpan.FromSeconds(3), 5)
               },
               null,
               BlockBlobUploadComplete,
               cloudBlob);

            cloudBlob.Properties.ContentType = mimeType;
            cloudBlob.Properties.CacheControl = "";
            cloudBlob.BeginSetProperties(ar => (ar.AsyncState as CloudBlockBlob).EndSetProperties(ar), cloudBlob);
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
   }
}

