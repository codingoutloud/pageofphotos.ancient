using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevPartners.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using PoP.Models;
using ValetKeyPattern.AzureStorage;

namespace MediaRepository
{
   public static class MediaIngester
   {
      /// <summary>
      /// Upload the blob and then (if nothing went wrong) drop a message on the queue announcing the blob
      /// </summary>
      /// <param name="queueValet"></param>
      /// <param name="mediaByteStream">Might be from File Upload via web page</param>
      /// <param name="origFilename"></param>
      /// <param name="mimeType"></param>
      /// <param name="byteCount">Count of bytes in the stream. Not used at this time. May be used in future to optimize the upload to blob storage, for telemetry, or to block uploads over a certain size.</param>
      /// <param name="destinationUrl"></param>
      /// <param name="blobValet"></param>
      public static void CaptureUploadedMedia(BlobValet blobValet, QueueValet queueValet, Stream mediaByteStream,
         string origFilename,
         string mimeType, int byteCount, string destinationUrl)
      {
         try
         {
            // TODO: obviate MediaStorageUrlFile.ExtTemplate by basing on MediaStorageValetKeyUrl value --- value="http://127.0.0.1:10000/devstoreaccount1/popmedia/{0}{1}" & "http://127.0.0.1:10000/devstoreaccount1/popmedia?sr=c&amp;si=open-wide-container-access-policy&amp;sig=X0yGw1Ydmu%2BCwk%2FTY7nj5HFgzv%2BIYg%2Bun%2BHQhNMmThk%3D"

#if false
               var destinationUrl =
                  String.Format(ConfigurationManager.AppSettings["MediaStorageUrlFile.ExtTemplate"],
                     Guid.NewGuid(),
                     new FileInfo(origFilename).Extension
                     );
               var valetKeyUrl = ConfigurationManager.AppSettings["MediaStorageValetKeyUrl"];

               // if that worked, notify via queue
               var mediaIngestionQueueValetKeyUrl = ConfigurationManager.AppSettings["MediaIngestionQueueValetKeyUrl"];
#endif
            blobValet.UploadStream(destinationUrl, mediaByteStream, mimeType);
            var info = new MediaUploadModel()
                       {
                          BlobUrl = destinationUrl,
                          Username = "codingoutloud"
                       };
            var queueMessage = new CloudQueueMessage(ByteArraySerializer<MediaUploadModel>.Serialize(info));
            queueValet.AddMessage(queueMessage); // send an arbitrary object on the queue, not just a string 
         }
         catch (StorageException ex)
         {
            System.Diagnostics.Trace.TraceError("Exception thrown in BlobExtensions.UploadFile: " + ex);
            throw;
         }
      }
   }
}


