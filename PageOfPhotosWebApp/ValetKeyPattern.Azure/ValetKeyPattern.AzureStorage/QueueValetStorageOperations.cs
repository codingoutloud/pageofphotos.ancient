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
   public static class QueueValetStorageOperations
   {
      /// <summary>
      /// Extension method that adds a message to the queue. Accepts all the same parameters that 
      /// CloudQueue.AddMessage accepts and passes them through.
      /// </summary>
      /// <param name="queueValet">QueueValet object on which this extension method is applied.</param>
      /// <param name="message">The message to add.</param>
      /// <param name="timeToLive">The maximum time to allow the message to be in the queue, or null.</param>
      /// <param name="initialVisibilityDelay">The length of time from now during which the message will be invisible. 
      /// If <c>null</c> then the message will be visible immediately.</param>
      /// <param name="options">An <see cref="T:Microsoft.WindowsAzure.Storage.Queue.QueueRequestOptions"/> object that 
      /// specifies any additional options for the request.</param>
      /// <param name="operationContext">An object that represents the context for the current operation.</param>
      public static void AddMessage(this QueueValet queueValet, CloudQueueMessage message,
                                    TimeSpan? timeToLive = null, TimeSpan? initialVisibilityDelay = null,
                                    QueueRequestOptions options = null, OperationContext operationContext = null)
      {
         try
         {
            var queueCreds = new StorageCredentials(queueValet.ValetKeyUri.Query);
            var queueName = queueValet.QueueName;
            var vkQueueClient = new CloudQueueClient(new Uri(String.Format("https://{0}", queueValet.ValetKeyUri.Host)), queueCreds);
            var queueRef = vkQueueClient.GetQueueReference(queueName);
            queueRef.AddMessage(message);
         }
         catch (StorageException ex)
         {
            System.Diagnostics.Trace.TraceError("Exception thrown: " + ex);
            throw;
         }
      }
   }
}

