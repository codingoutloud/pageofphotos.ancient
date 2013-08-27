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
      public static void CaptureUploadedMedia(Stream mediaByteStream, string origFilename, string origMimeType, int byteCount)
      {
         {
             try
             {
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
                 var queueValetKeyUri = new Uri(mediaIngestionQueueValetKeyUrl);
                 var queueCreds = StorageCredentialsSwizzler.CreateFromUri(queueValetKeyUri);
                 var queueClient = new CloudQueueClient(StorageCredentialsSwizzler.QueueBaseUri(queueValetKeyUri), queueCreds);
                 var queueMessage = new CloudQueueMessage(destinationUrl);
                 var queueName = StorageCredentialsSwizzler.QueueName(queueValetKeyUri);
                 queueClient.GetQueueReference(queueName).AddMessage(queueMessage);
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
