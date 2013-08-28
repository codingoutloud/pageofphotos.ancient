using System.Configuration;
using System.Diagnostics;
using WindowsAzureStorageCredentialsSwizzler;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRepository
{
   public static class UriExtensions // TODO: factor out of this project
   {
      public static string PathNoQuery(this Uri uri)
      {
         var path = uri.ToString();
         var queryStart = path.IndexOf("?", StringComparison.InvariantCulture);

         return queryStart > 0 ? path.Substring(0, queryStart) : path;
      }
   }

   public static class AzureStorageHelper // TODO: this is a terrible class name
   {
      /// <summary>
      /// Upload the blob and then (if nothing went wrong) drop a message on the queue announcing the blob
      /// </summary>
      /// <param name="mediaByteStream">Might be from File Upload via web page</param>
      /// <param name="origFilename"></param>
      /// <param name="origMimeType"></param>
      /// <param name="byteCount"></param>
      public static void CaptureUploadedMedia(Stream mediaByteStream, string origFilename, string origMimeType,
         int byteCount)
      {
         {
            try
            {
               // TODO: obviate MediaStorageUrlFile.ExtTemplate by basing on MediaStorageValetKeyUrl value --- value="http://127.0.0.1:10000/devstoreaccount1/popmedia/{0}{1}" & "http://127.0.0.1:10000/devstoreaccount1/popmedia?sr=c&amp;si=open-wide-container-access-policy&amp;sig=X0yGw1Ydmu%2BCwk%2FTY7nj5HFgzv%2BIYg%2Bun%2BHQhNMmThk%3D"

               var destinationUrl =
                  String.Format(ConfigurationManager.AppSettings["MediaStorageUrlFile.ExtTemplate"],
                     Guid.NewGuid(),
                     new FileInfo(origFilename).Extension
                     );

               var valetKeyUrl = ConfigurationManager.AppSettings["MediaStorageValetKeyUrl"];
               var valetKeyUri = new Uri(valetKeyUrl);
               var creds = StorageCredentialsSwizzler.CreateFromUrl(valetKeyUrl);
               var blob = new CloudBlockBlob(new Uri(destinationUrl), creds);
               blob.UploadFromStream(mediaByteStream,
                  options:
                     new BlobRequestOptions
                     {
                        RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(3), 5)
                     });

               // if that worked, notify via queue
               var mediaIngestionQueueValetKeyUrl = ConfigurationManager.AppSettings["MediaIngestionQueueValetKeyUrl"];

             //TODO: ENCAPSULATE IN A QueueValet Class:
               var queueCreds = StorageCredentialsSwizzler.CreateFromUrl(mediaIngestionQueueValetKeyUrl);
               var queueClient = new CloudQueueClient(StorageCredentialsSwizzler.QueueBaseUri(mediaIngestionQueueValetKeyUrl), queueCreds);
               var queueMessage = new CloudQueueMessage(destinationUrl);
               var queueName = StorageCredentialsSwizzler.QueueName(mediaIngestionQueueValetKeyUrl);
               var queueRef = queueClient.GetQueueReference(queueName);
               queueRef.AddMessage(queueMessage);
            }
            catch (StorageException ex)
            {
               System.Diagnostics.Trace.TraceError("Exception thrown in BlobExtensions.UploadFile: " + ex);
               throw;
            }
         }
      }
   }
}

