using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace ValetKeyPattern.AzureStorage
{
   public static class BlobContainerValetOperations
   {
      /// <summary>
      /// Upload the blob and then (if nothing went wrong) drop a message on the queue announcing the blob
      /// </summary>
      /// <param name="byteStream">Might be from File Upload via web page</param>
      /// <param name="origFilename"></param>
      /// <param name="origMimeType"></param>
      /// <param name="byteCount">Count of bytes in the stream. Not used at this time. May be used in future to optimize the upload to blob storage, for telemetry, or to block uploads over a certain size.</param>
      public static void UploadStreamToBlob(this BlobContainerValet blobContainerValet, Stream byteStream, string origFilename, string origMimeType, int byteCount)
      {

#if false
      /// <summary>
      /// Upload the blob and then (if nothing went wrong) drop a message on the queue announcing the blob
      /// </summary>
      /// <param name="mediaByteStream">Might be from File Upload via web page</param>
      /// <param name="origFilename"></param>
      /// <param name="origMimeType"></param>
      /// <param name="byteCount">Count of bytes in the stream. Not used at this time. May be used in future to optimize the upload to blob storage, for telemetry, or to block uploads over a certain size.</param>
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
#endif
#if false
         {
            try
            {
               var blob = new CloudBlockBlob(new Uri(destinationUrl), creds);
               blob.UploadFromStream(byteStream,
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
#endif
      }
   }
}
