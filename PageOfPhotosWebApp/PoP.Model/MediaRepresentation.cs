using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace PoP.Model
{
   public abstract class MediaRepresentation
   {
      [Key]
      public string Id { get; set; } // is this media id? or userid?
      public string Title { get; set; }
      public string Url { get; set; }
      public string MimeType { get; set; }
      public Size DisplaySize { get; set; }
   }

   public class ImageRepresentation : MediaRepresentation
   {
      public ImageRepresentation() : base() { }
   }
}
