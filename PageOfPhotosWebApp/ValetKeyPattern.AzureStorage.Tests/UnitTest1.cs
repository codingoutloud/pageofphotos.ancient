using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ValetKeyPattern.AzureStorage.Tests
{
   [TestClass]
   public class AddressingCloudStorageTests
   {
      [TestMethod]
      public void TestAddressingCloudStorage1()
      {
         var url = "http://127.0.0.1:10000/account-name/resource";
         Assert.IsTrue(new Uri(url).IsEmulated());
         url = "http://127.0.0.1:10001/account-name/resource";
         Assert.IsTrue(new Uri(url).IsEmulated());
         url = "http://127.0.0.1:10002/account-name/resource";
         Assert.IsTrue(new Uri(url).IsEmulated());
      }

      [TestMethod()]
      [ExpectedExceptionAttribute(typeof(ArgumentException))]
      public void TestAddressingCloudStorage2_ShouldHttpsOnLocalStorageThrowException()
      {
         var url = "https://127.0.0.1:10000/account-name/resource";
         var isCloud = new Uri(url).IsEmulated();
         Assert.Fail("should never get here since https to local storage should raise exception");
      }

      [TestMethod]
      public void TestAddressingCloudStorage3()
      {
         var url = "http://accountname.queue.core.windows.net/queuename?SomeQueryStringForNow";
         Assert.IsFalse(new Uri(url).IsEmulated());
         url = "http://accountname.blob.core.windows.net/containername/containerfile?SomeQueryStringForNow";
         Assert.IsFalse(new Uri(url).IsEmulated());
         url = "http://accountname.table.core.windows.net/tablename?SomeQueryStringForNow";
         Assert.IsFalse(new Uri(url).IsEmulated());

         url = "https://accountname.queue.core.windows.net/queuename?SomeQueryStringForNow";
         Assert.IsFalse(new Uri(url).IsEmulated());
         url = "https://accountname.blob.core.windows.net/containername/containerfile?SomeQueryStringForNow";
         Assert.IsFalse(new Uri(url).IsEmulated());
         url = "https://accountname.table.core.windows.net/tablename?SomeQueryStringForNow";
         Assert.IsFalse(new Uri(url).IsEmulated());
      }
   }
}
