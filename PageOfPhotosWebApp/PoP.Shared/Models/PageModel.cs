using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace PoP.WebTier.Models
{
   public class PageModel : SlugModel
   {
      public List<MediaRepresentation> Media { get; set; }
   }

   public class SlugModel
   {
      [Key]
      public string Name { get; set; }
      public string Description { get; set; }
   }

   public abstract class MediaRepresentation
   {
      [Key]
      public string Id { get; set; }
      public string Title { get; set; }
      public string Url { get; set; }
      public string MimeType { get; set; }
      public Size DisplaySize { get; set; }
   }

   public class ImageRepresentation : MediaRepresentation
   {
      public ImageRepresentation() : base() {}
   }

   public class PageDetailModel
   {

   }
}
