using PoP.WebTier.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageRepository
{
    public static class PageBuilder // TODO: extract interface
    {
        public static PageModel GetPageForSlug(string slug)
        {
            var page = new PageModel();
            page.Description = "Page Descript";
            page.Name = "Page Name Here";
            page.Media = new List<MediaRepresentation>(); // empty
            return page;
        }
    }

    public static class PageLister // TODO: extract interface
    {
        public static List<SlugModel> ListPages()
        {
            var slug1 = new SlugModel { Name = "kevin", Description = "Kevin likez the Bruins" };
            var slug2 = new SlugModel { Name = "bill", Description = "Bill is a nerd" };
            var slug3 = new SlugModel { Name = "timothy", Description = "T.J. likes anime" };
            var slugList = new List<SlugModel> { slug1, slug2, slug3 };
            return slugList;
        }
    }
}
