using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PoP.WebTier
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

#if OutOfTheBoxRoute
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
#else
            routes.MapRoute(
               name: "Display Page of Photos for the provided slug, else list of slugs",
               url: "{slug}",
               defaults: new { controller = "Page", action = "Index" }
                // TODO: add constraints: to not greedily grab other controller prefixes (and matching ones should be made illegal slug values)
               );

            routes.MapRoute(
               name: "File Management",
               url: "Page/{action}",
               defaults: new { controller = "Page" }
               );

            routes.MapRoute(
               name: "Default",
               url: "{controller}/{action}/{id}",
               defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
               );
#endif
        }
    }
}
