using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using DevPartners.Azure;
using MediaRepository.Table;
using Microsoft.WindowsAzure.Storage;
using ValetKeyPattern.AzureStorage;
using PoP.Models;

namespace PoP.ServiceTier
{
   public class NewMediaProcessor
   {
      public static void ProcessNextMediaMessage(QueueValet queueValet, BlobValet blobValet)
      {
         try
         {
            var msg = queueValet.GetMessage(TimeSpan.FromMinutes(5));

            if (msg != null)
            {
               if (msg.DequeueCount <= 1)
                  Trace.TraceInformation("DequeueCount = {0}", msg.DequeueCount);
               else
                  Trace.TraceWarning("DequeueCount = {0}", msg.DequeueCount);

//TODO:               if (msg.DequeueCount > 5) return;

               // Is it a photo or video? -- only photo supported for now... so assume that
               // (alternatively, might have a photo queue and a video queue)

               var mediaUploadInfo = ByteArraySerializer<MediaUploadModel>.Deserialize(msg.AsBytes);

               var uploadedPhotoUrl = mediaUploadInfo.BlobUrl;
               var username = mediaUploadInfo.Username;

               ProcessNextPhotoUpload(uploadedPhotoUrl, blobValet);

               queueValet.DeleteMessage(msg);

               // TODO: Ensure proper Poison Message Handling
            }
         }
         catch (Exception ex)
         {
            // this method CANNOT throw an exception - that's bad manners - even if there is a failure
            // SEE: Queue-Centric Workflow Pattern (chapter 3) plus Poison Message and Reliable Queue concepts
            // SEE: Strong Exception Guarantee http://en.wikipedia.org/wiki/Exception_safety 
            // or relate to the NoFail Guaranteed from http://c2.com/cgi/wiki?ExceptionGuarantee

            var debugMsg =
               String.Format("Exception in PoP.ServiceTier.NewMediaProcessor.ProcessNextMediaMessage [{0}]\n[{1}]",
                  ex.GetBaseException(), ex);
            Trace.TraceError(debugMsg);
         }
      }

      /// <summary>
      /// Turns guid.png into guid_thumb.png
      /// </summary>
      /// <param name="url"></param>
      /// <returns>Original name with _thumb inserted</returns>
      internal static string BuildThumbnailVersionOfBlobUrl(string url)
      {
         // since we know this is a blob url going old-school here - we'll just swizzle this as a string
         var ext = Path.GetExtension(url);
         Contract.Assert(ext != null);
         Contract.Assert(ext.IndexOf('.', 0) == 0);
         Contract.Assert(url.LastIndexOf(ext, StringComparison.Ordinal) == url.IndexOf(ext, StringComparison.Ordinal));
         return url.Replace(ext, "_thumb" + ext);
      }


      private static void ProcessNextPhotoUpload(string origImageUrl, BlobValet blobValet)
      {
         var httpClient = new HttpClient();
         var origStream = httpClient.GetStreamAsync(origImageUrl).Result;
         var origMime = blobValet.GetSupportedMimeTypeFromFileName(origImageUrl);
         var origImage = Image.FromStream(origStream);

         // create thumb version of image
         var thumbUrl = BuildThumbnailVersionOfBlobUrl(origImageUrl);
         var thumbUri = new Uri(thumbUrl);
         var thumbMime = origMime;
#if true
         var thumbStream = MediaFormatter.PopImageThumbnailer.GetThumbnailStream(origImage, thumbMime);
#else
         var thumb = MediaFormatter.PopImageThumbnailer.GetThumbnail(origImage);
         thumb.Save(@"d:\temp\foo.png");
         var thumbStream = File.Open(@"d:\temp\foo.png", FileMode.Open);
#endif

#if true
         blobValet.UploadStream(thumbUri, thumbStream, thumbMime);
#else
         using (var thumbStream2 = new MemoryStream())
         {
            origImage.GetThumbnailImage(100, 100, () => false, IntPtr.Zero).Save(thumbStream2, ImageFormat.Jpeg);
            thumbStream2.Position = 0;
            blobValet.UploadStream(thumbUri, thumbStream2);
         }
#endif

         // now attach it to an account
         var userMediaRepo = new UserMediaRepository(CloudStorageAccount.DevelopmentStorageAccount, "usermedia");
         var userMedia = new UserMedia(1, new Random().Next(3, 4315))
         {
            StorageFormat = origMime,
            Url = origImageUrl,
            ThumbUrl = thumbUrl,
            // ETag = "*",  // TODO: << == bad idea, fix
            UserName = "codingoutloud" // TODO: << may not want to store this long-term since we have UserId (in SQL Azure)
         };
         userMediaRepo.Insert(userMedia);
      }
   }
}

