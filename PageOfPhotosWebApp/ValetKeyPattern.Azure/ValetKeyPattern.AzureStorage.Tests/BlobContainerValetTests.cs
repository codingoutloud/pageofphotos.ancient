using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ValetKeyPattern.AzureStorage.Tests
{
   [TestClass]
   public class BlobContainerValetTests
   {
       private static string ValetKeyUrl =
          "https://boxesbackups.blob.core.windows.net/test-sas-updates-destination?sr=c&sv=2012-02-12&st=2013-07-27T22%3A08%3A17Z&se=2023-07-27T23%3A08%3A00Z&sp=rwdl&sig=uecb9Dxkf5bWpD6u4RB3JfVpV%2F14QcHNdoQv%2FV3WLXg%3D";

      private static string PublicFileUrl1 =
         "https://boxesbackups.blob.core.windows.net/test-sas-updates-source/bill.png?sr=b&sv=2012-02-12&st=2013-07-27T22%3A09%3A00Z&se=2023-07-27T23%3A09%3A00Z&sp=r&sig=l2KDiRV1PLSxhr0ncKkkCLaKnkdAVZmdVqJWH%2BYxhWw%3D";

      private static string PublicFileUrl2 =
         "https://boxesbackups.blob.core.windows.net/test-sas-updates-source/foo.txt?sr=b&sv=2012-02-12&st=2013-07-27T22%3A10%3A05Z&se=2023-07-27T23%3A10%3A00Z&sp=rwd&sig=5SU4owZv5Vx52B1s%2B4c90d8N18zhzdh4JOK3OW%2F3zqc%3D";

      private static string PublicFileUrl3 =
         "https://api.github.com/repos/squdgy/dfst_webapi_azure/contents/DFST_WebApiCloud/DfstWebApiAzure/SQLQuery1.sql?access_token=b2d36e11c97a56985a8c9daf494c38aa6adc9d9c";

      private static string DestinationUrl1 =
         "https://boxesbackups.blob.core.windows.net/test-sas-updates-destination/bill.png";
     
      [TestMethod]
      public void ValetKey_GetPathFromValidValetKey_Succeeds()
      {
         var uri = BlobContainerValet.GetDestinationPathFromValetKey(ValetKeyUrl);
         Assert.IsNotNull(uri);
      }

      [TestMethod]
      public void ValetKey_GetFileFromValidFileName_Succeeds()
      {
         var expected = "expected.txt";
         var source = @"c:\foo\bar\" + expected;

         var result = BlobContainerValet.GetFileNameFromUrl(source);
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void ValetKey_GetFileFromValidFileUrl_Succeeds()
      {
         var expected = "expected.txt";
         var source = @"http://foo/bar/xyz/" + expected;

         var result = BlobContainerValet.GetFileNameFromUrl(source);
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void ValetKey_GetSasTokenFromSasUrl_Succeeds()
      {
         var sasToken = BlobContainerValet.GetSasTokenFromValetKeyUrl(ValetKeyUrl);
         Assert.IsNotNull(sasToken);
         Assert.IsTrue(sasToken[0] == '?');
      }

      [TestMethod]
      public void ValetKey_BuildDestinationUri_RetainsContainerName()
      {
         var destinationUri = BlobContainerValet.BuildDesinationUri(ValetKeyUrl, PublicFileUrl1);
         Assert.AreEqual(destinationUri.AbsoluteUri, DestinationUrl1);
      }

      [TestMethod]
      [TestCategory("Integration")]
      public void ValetKey_UpdateBlobWithinContainer_Succeeds()
      {
         BlobContainerValet.UploadToBlobContainer(ValetKeyUrl, PublicFileUrl1);
         BlobContainerValet.UploadToBlobContainer(ValetKeyUrl, PublicFileUrl2);
         BlobContainerValet.UploadToBlobContainer(ValetKeyUrl, PublicFileUrl1);
         BlobContainerValet.UploadToBlobContainer(ValetKeyUrl, PublicFileUrl3);

         // should be up there!
      }
   }
}
