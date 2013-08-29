using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoP.Models
{
   /// <summary>
   /// Useful for sending related info in a queue message when new media has been uploaded
   /// </summary>
   [Serializable]
   public class MediaUploadModel
   {
      public string BlobUrl { get; set; }
      public string Username { get; set; } // owner of the media at BlobUrl
   }
}
