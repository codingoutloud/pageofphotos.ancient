using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRepository
{
   public static class UriExtensions
   {
      public static string PathNoQuery(this Uri uri)
      {
         var path = uri.ToString();
         var queryStart = path.IndexOf("?", StringComparison.InvariantCulture);

         if (queryStart > 0)
         {
            return path.Substring(0, queryStart);
         }
         else
         {
            return path;
         }
      }
   }

   public static class AzureStorageHelper
   {
      public static void CaptureUploadedMedia(Stream mediaByteStream, string origFilename, string origMimeType, int byteCount)
      {
         {
            try
            {
                var valetKeyUrl = ConfigurationManager.AppSettings["MediaStorageValetKeyUrl"];
                var destinationUrl =
                    String.Format(ConfigurationManager.AppSettings["MediaStorageUrlFile.ExtTemplate"],
                    Guid.NewGuid(),
                    new FileInfo(origFilename).Extension
                    );

               var valetKeyUri = new Uri(valetKeyUrl);
               var creds = new StorageCredentials(valetKeyUri.Query);
               var blob = new CloudBlockBlob(new Uri(destinationUrl), creds);
               blob.UploadFromStream(mediaByteStream, options: new BlobRequestOptions { RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(3), 5) });

               // if that worked, notify via queue
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
