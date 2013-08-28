using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;

namespace ValetKeyPattern.AzureStorage
{
   public class QueueValet : AzureStorageValet
   {
      public string QueueName { get { return ValetKeyUri.AbsolutePath.Substring(1); } }
      private const bool AutoRepairDefault = true;
      private const string UrlSchemeDefault = "https";
      public Uri ValetKeyUri { get; private set; }
      // TODO: add bool AutoRepair { get; set; }

      public QueueValet(Uri valetKeyUri) // : base(uri.AbsoluteUri)
      {
         ValetKeyUri = valetKeyUri;
      }

#if false
      public QueueValet CreateQueueValetUri(string storageAccountName, string queueName, string sasQueryString,
                                               string urlScheme = UrlSchemeDefault, bool autorepair = AutoRepairDefault)
      {
         // TODO: this will FAIL for local dev environment
         var uri = new Uri(String.Format("{0}://{1}.queue.core.windows.net/{2}{3}", urlScheme, storageAccountName, queueName, sasQueryString));
         return new QueueValet(uri);
      }
#endif

      public bool IsValidValetUrl(string url)
      {
         return IsValidValetUri(new Uri(url));
      }

      public bool IsValidValetUri(Uri uri)
      {
         // TODO: add more tests
         return uri.Query.Length > 1;
      }
   }
}

