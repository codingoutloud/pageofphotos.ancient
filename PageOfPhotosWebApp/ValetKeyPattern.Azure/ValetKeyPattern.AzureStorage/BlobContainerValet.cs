using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ValetKeyPattern.AzureStorage
{
    public class BlobContainerValet
    {
       public static Uri GetDestinationPathFromValetKey(string valetKeyUrl)
       {
          var valetKeyUri = new Uri(valetKeyUrl);
          var hostUri = new Uri(String.Format("{0}://{1}", valetKeyUri.Scheme, valetKeyUri.Host));
          return new Uri(hostUri, valetKeyUri.LocalPath);
       }

       /// <summary>
       /// From c:\x\y\foo.txt, return foo.txt
       /// From http://example.com/x/y/foo.txt, return foo.txt
       /// </summary>
       /// <param name="sourcePath">Source of file name. Either a URL or local file path.</param>
       /// <returns></returns>
       public static string GetFileNameFromUrl(string sourcePath)
       {
          var pathUri = new Uri(sourcePath);
          var filePath = pathUri.LocalPath;
          var fileInfo = new FileInfo(filePath);
          return fileInfo.Name;
       }

       public static Uri BuildDesinationUri(string valetKeyUrl, string sourceFilePath)
       {
          // FAILS (drops path on first param): return new Uri(GetDestinationPathFromValetKey(valetKeyUrl), GetFileNameFromUrl(sourceFilePath));
          // SUCCEEDS: return new Uri(String.Format("{0}/{1}", GetDestinationPathFromValetKey(valetKeyUrl), GetFileNameFromUrl(sourceFilePath));
          return new Uri(String.Format("{0}/{1}", GetDestinationPathFromValetKey(valetKeyUrl), GetFileNameFromUrl(sourceFilePath)));
       }

       public static string GetSasTokenFromValetKeyUrl(string valetKeyUrl)
       {
          var valetKeyUri = new Uri(valetKeyUrl);
          return valetKeyUri.Query;
       }

       /// <summary>
       /// Clean up stream object.
       /// </summary>
       /// <param name="result"></param>
       private static void BlockBlobUploadComplete(IAsyncResult result)
       {
          var blockBlob = (CloudBlockBlob) result.AsyncState;
          blockBlob.EndUploadFromStream(result);
       }

       /// <summary>
       /// Given a Valet Key which allows writing/creating blobs for a Blob Container,
       /// write the given file into the equivalently-named blob.
       /// Currently ignores situations where the source filename is not allowed as the blob name.
       /// </summary>
       public static void UploadToBlobContainer(string valetKeyUrl, string sourceFilePath)
       {
          var desinationUri = BuildDesinationUri(valetKeyUrl, sourceFilePath);
          var sasToken = GetSasTokenFromValetKeyUrl(valetKeyUrl);
          var credentials= new StorageCredentials(sasToken);
          var cloudBlob = new CloudBlockBlob(desinationUri, credentials);

          try
          {
             string acceptHeaderValue = null;
             if (sourceFilePath.Contains("https://api.github.com"))
             {
                acceptHeaderValue = "application/vnd.github.raw"; // TODO: this path has no specific unit tests
             }

             // TODO: if already exists, preserve Content Type (mime-type) and Cache Control values ()
             // TODO: ... and probably Content Encoding (e.g., "gzip") and Content Language (e.g., "da" or "de, en") values
             // TODO: consider setting mime-type, caching, other headers on first version
             // TODO: could allow snapshotting when not first version
             // TODO: could become async for scale and reliability
             // TODO: consider destinationBlob.StartCopyFromBlob(sourceFilePath) for async copy
             // TODO: also support files (vs HTTP only): using (var stream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
             using (var httpClient = new HttpClient())
             {
                if (!String.IsNullOrEmpty(acceptHeaderValue))
                {
                   httpClient.DefaultRequestHeaders.Add("Accept", acceptHeaderValue);
                }
                var stream = httpClient.GetStreamAsync(sourceFilePath).Result;
                cloudBlob.BeginUploadFromStream(stream, BlockBlobUploadComplete, cloudBlob);
             }
          }
          catch (StorageException ex)
          {
             // TODO: dude - how about some error handling?
             Trace.TraceError(ex.ToString());
             throw;
          }
       }
    }
}
