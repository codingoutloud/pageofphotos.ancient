using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MediaFormatter
{
   public class PopImageThumbnailer // TODO: really ought to be just generic Thumbnailer class with constructor that accepts list of allowed mime types
   {
      public static Image GetThumbnail(Image origImage)
      {
         return origImage.GetThumbnailImage(100, 100, () => false, IntPtr.Zero);
      }

      // TODO: keep the original aspect ratio
      public static Stream GetThumbnailStream(Image origImage, string mimeType)
      {
         var thumbStream = ConvertImageToStream(origImage.GetThumbnailImage(100, 100, () => false, IntPtr.Zero), mimeType);        
         return thumbStream;
      }

      public static Stream ConvertImageToStream(Image image, string mimeType)
      {
         var ms = new MemoryStream();
         // following line also works in lieue of image.Save - pros and cons?
         // new BinaryFormatter().Serialize(ms, image);
         image.Save(ms, MapMimeTypeToImageFormat(mimeType));
         ms.Position = 0; // else we'll get Microsoft.WindowsAzure.Storage.StorageException, HResult=-2146233088, Message=Cannot access a closed file.

         return ms;
      }      

      // TODO: Align this with BlobValet.GetSupportedMimeTypeFromFileName
      internal static ImageFormat MapMimeTypeToImageFormat(string mimeType)
      {
         if (mimeType == "image/png") return ImageFormat.Png;
         if (mimeType == "image/jpeg") return ImageFormat.Jpeg;
         throw new ArgumentException("Unknown or unsupported ImageFormat for Mime-Type " + mimeType, "mimeType");
      }
   }
}
