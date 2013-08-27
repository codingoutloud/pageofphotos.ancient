using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WindowsAzureStorageCredentialsSwizzler.Tests.Unit
{
   [TestClass]
   public class CreateFromUrlTests
   {
      [TestMethod]
      public void CreateFromUrlTest1()
      {
         var url = "http://127.0.0.1:10000/account-name/resource";
         var creds = StorageCredentialsSwizzler.CreateFromUrl(url);

         Assert.IsTrue(creds.AccountName == Constants.CloudEmulatorStorageAccountName);
      }

      [TestMethod]
      public void CreateFromUrlTest2()
      {
         var url = "https://popmedia.blob.core.windows.net/popmedia?sr=c&sv=2012-02-12&st=2011-08-27T20%3A00%3A00Z&se=2013-08-27T20%3A00%3A00Z&sp=rwdl&sig=75kpdMAvaraAfjxHUOpVMIaB%2Bze7MgCojKKwMl0NGFg%3D";
         var creds = StorageCredentialsSwizzler.CreateFromUrl(url);

         Assert.IsTrue(creds.SASToken == new Uri(url).Query);
      }
   }
}
