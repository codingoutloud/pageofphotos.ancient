using System.ComponentModel.DataAnnotations;

namespace PoP.Model
{
   public class SlugModel
   {
      [Key]
      public string Name { get; set; }
      public string Description { get; set; }
   }
}
