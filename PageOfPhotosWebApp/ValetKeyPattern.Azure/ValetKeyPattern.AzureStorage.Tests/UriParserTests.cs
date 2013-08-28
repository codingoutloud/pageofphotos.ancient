using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming

namespace ValetKeyPattern.AzureStorage.Tests
{
   [TestClass]
   public class UriParserTests
   {
      [TestMethod]
      public void UriParser_ParsingWellFormedMinimalCloudStoragePartitionName_Succeeds()
      {
         var expected = "partition";
         var url = "http://accountname.queue.core.windows.net/partition?SomeQueryStringForNow";
         var uri = new Uri(url);

         Assert.AreEqual(expected, uri.StoragePartitionName());
      }

      [TestMethod]
      public void UriParser_ParsingWellFormedComplexCloudStoragePartitionName_Succeeds()
      {
         var expected = "partition";
         var url = "http://accountname.queue.core.windows.net/partition/somethingelse?SomeQueryStringForNow";
         var uri = new Uri(url);

         Assert.AreEqual(expected, uri.StoragePartitionName());
      }

      [TestMethod]
      public void UriParser_ParsingWellFormedComplexLocalStoragePartitionName_Succeeds()
      {
         var expected = "partition";
         var url = "http://127.0.0.1:10000/account-name/partition/somethingelse";
         var uri = new Uri(url);

         Assert.AreEqual(expected, uri.StoragePartitionName());
      }

      [TestMethod]
      public void UriParser_ParsingWellFormedMinimalLocalStoragePartitionName_Succeeds()
      {
         var expected = "partition";
         var url = "http://127.0.0.1:10000/account-name/partition";
         var uri = new Uri(url);

         Assert.AreEqual(expected, uri.StoragePartitionName());
      }
   }
}

// ReSharper restore InconsistentNaming
