using System;
using System.IO;
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
            blobValet.UploadStream(new Uri(destinationUrl), mediaByteStream, mimeType); // TODO: at moment is sync (not async) to avoid race condition mentioned below
            var info = new MediaUploadModel
            {
               BlobUrl = destinationUrl,
               Username = "codingoutloud"
            };

            // prep  an arbitrary object to send on the queue, not just a string (not rich enough for our use case)
            var queueMessage = new CloudQueueMessage(ByteArraySerializer<MediaUploadModel>.Serialize(info));

            // TODO: race condition when both uploading a BLOB and posting the Queue message - the queue message processing
            // TODO: ... can begin before the blob upload is complete -- need to sync these
            // TODO: ... BUT! for now it will still FUNCTION CORRECTLY (if inefficiently) due to Queue-Centric Workflow Pattern retries IF not determined to be a Poison Message

            // There is no real need for a 50ms delay before the message can appear in queue, but just showing how to do it.
            // Technique is sometimes useful when there's a reason to delay its processing. You could use it to implement a
            // scheduler, for example. In the case of PoP, there are no obvious use cases. A made-up use case might be if PoP
            // introduced a way to make photos show up in the future allowing the user uploading them to indicate PoP should
            // delay processing for, say, up to 24 hours, and let the user optionally specify a delay within that range.
            queueValet.AddMessage(queueMessage, initialVisibilityDelay: TimeSpan.FromMilliseconds(50));
         }
         catch (StorageException ex)
         {
            System.Diagnostics.Trace.TraceError("Exception thrown in BlobExtensions.UploadFile: " + ex);
            throw;
         }
      }
   }
}


