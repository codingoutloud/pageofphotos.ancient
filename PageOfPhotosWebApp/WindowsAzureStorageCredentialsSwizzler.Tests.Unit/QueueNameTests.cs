using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WindowsAzureStorageCredentialsSwizzler.Tests.Unit
{
   [TestClass]
   public class QueueNameTests
   {
      [TestMethod]
      public void QueueNameTest1()
      {
         var url = "http://127.0.0.1:10000/accountname/queuename";
         Assert.AreEqual("queuename", StorageCredentialsSwizzler.QueueName(url));
      }

      [TestMethod]
      public void QueueNameTest2()
      {
         var url = "http://accountname.queue.core.windows.net/queuename?SomeQueryStringForNow";
         Assert.AreEqual("queuename", StorageCredentialsSwizzler.QueueName(url));
      }
   }
}
