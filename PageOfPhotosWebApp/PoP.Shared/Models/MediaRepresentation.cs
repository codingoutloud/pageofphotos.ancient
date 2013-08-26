using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoP.Shared.Models
{
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
      public ImageRepresentation() : base() { }
   }
}
