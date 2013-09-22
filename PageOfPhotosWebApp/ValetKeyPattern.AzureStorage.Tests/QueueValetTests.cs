using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming

namespace ValetKeyPattern.AzureStorage.Tests
{
   [TestClass]
   public class QueueValetTests
   {
      [TestMethod]
      public void QueueNameTest1()
      {
         var url = "http://127.0.0.1:10000/accountname/queuename";
         Assert.AreEqual("queuename", new QueueValet(url).QueueName);
      }

      [TestMethod]
      public void QueueNameTest2()
      {
         var url = "http://accountname.queue.core.windows.net/queuename?SomeQueryStringForNow";
         Assert.AreEqual("queuename", new QueueValet(url).QueueName);
      }
   }
}

// ReSharper restore InconsistentNaming
