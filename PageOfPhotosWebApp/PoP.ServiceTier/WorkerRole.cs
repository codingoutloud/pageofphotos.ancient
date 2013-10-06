using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Blob;
using ValetKeyPattern.AzureStorage;

namespace PoP.ServiceTier
{
   public class WorkerRole : RoleEntryPoint
   {
      public override void Run()
      {
         try
         {
            Trace.TraceInformation("PoP.ServiceTier entry point called");

            var queueValetKeyUrl = ConfigurationManager.AppSettings["MediaIngestionQueueValetKeyUrl"];
            var queueValet = new QueueValet(queueValetKeyUrl);

            var blobValetKeyUrl = ConfigurationManager.AppSettings["MediaStorageValetKeyUrl"];
            var blobValet = new BlobValet(blobValetKeyUrl);

            InitStorage(queueValet, blobValet);

            while (true)
            {
               Trace.TraceInformation("Working on the next iteration from PoP.ServiceTier.Run");

               NewMediaProcessor.ProcessNextMediaMessage(queueValet, blobValet);
               Thread.Sleep(TimeSpan.FromSeconds(2));
            }
         }
         catch (Exception)
         {
            Trace.TraceError("PoP.ServiceTier.Run has detected an uncaught exception was thrown - this should never happen", "Error");
            throw;
         }
      }

      private static void InitStorage(QueueValet queueValet, BlobValet blobValet)
      {
          queueValet.CreateQueue("media-ingestion");
          blobValet.CreateBlobContainer("popmedia", true);
          var container = blobValet.CreateBlobContainer("popuploads", false);

          string sasPolicy = BlobValet.CreateBlobSASPolicy(
              container,
              "untrusted-uploader",
              DateTime.UtcNow.AddYears(10),
              new SharedAccessBlobPermissions[] { SharedAccessBlobPermissions.Write }
          );
      }

      public override bool OnStart()
      {
         // Set the maximum number of concurrent connections 
         ServicePointManager.DefaultConnectionLimit = 12;

         // For information on handling configuration changes
         // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

         return base.OnStart();
      }
   }
}
