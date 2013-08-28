using System.ComponentModel.DataAnnotations;

namespace PoP.Models
{
   public class SlugModel
   {
      [Key]
      public string Name { get; set; }
      public string Description { get; set; }
   }
}
