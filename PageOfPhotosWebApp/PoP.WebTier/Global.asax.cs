using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PoP.WebTier
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
           var url = HttpContext.Current.Request.Url;
           
           var ex = Server.GetLastError();
            if (ex is ThreadAbortException || ex is HttpException)
                return;
            
            // Log AppErrETW

            // http://stackoverflow.com/questions/1171035/asp-net-mvc-custom-error-handling-application-error-global-asax
            // http://msdn.microsoft.com/en-us/library/24395wz3(v=vs.100).aspx
            // http://stackoverflow.com/questions/6508415/application-error-not-firing-when-customerrors-on/9572858#9572858

//            Response.Redirect("unexpectederror.htm");
            // HttpContext context = ((HttpApplication)sender).Context;
            if (HttpContext.Current != null)
            {
                var page = HttpContext.Current.Handler as System.Web.UI.Page;
            }
        }
    }
}