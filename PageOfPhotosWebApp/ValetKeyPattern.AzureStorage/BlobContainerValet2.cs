using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace ValetKeyPattern.AzureStorage
{
    public class BlobValet : AzureStorageValet
    {
    }

    public class BlobContainerValet2
    {
        public static Uri GetDestinationPathFromValetKey(string valetKeyUrl)
        {
            var valetKeyUri = new Uri(valetKeyUrl);
            var hostUri = new Uri(String.Format("{0}://{1}", valetKeyUri.Scheme, valetKeyUri.Host));
            return new Uri(hostUri, valetKeyUri.LocalPath);
        }

        public static Uri BuildDesinationUri(string valetKeyUrl, string sourceFilePath)
        {
            // FAILS (drops path on first param): return new Uri(GetDestinationPathFromValetKey(valetKeyUrl), GetFileNameFromUrl(sourceFilePath));
            // SUCCEEDS: return new Uri(String.Format("{0}/{1}", GetDestinationPathFromValetKey(valetKeyUrl), GetFileNameFromUrl(sourceFilePath));
            return
                new Uri(String.Format("{0}/{1}", GetDestinationPathFromValetKey(valetKeyUrl), sourceFilePath.FileName()));
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
        /// Infer from Uri version
        /// </summary>
        public static void UploadStream(string valetKeyUrl, string destinationUrl, Stream stream)
        {
            UploadStream(new Uri(valetKeyUrl), new Uri(destinationUrl), stream);
        }

        /// <summary>
        /// </summary>
        /// <param name="valetKeyUri"></param>
        /// <param name="destinationUri"></param>
        /// <param name="stream"></param>
        public static void UploadStream(Uri valetKeyUri, Uri destinationUri, Stream stream)
        {
            var sasToken = GetSasTokenFromValetKeyUrl(valetKeyUri.AbsoluteUri);
            var credentials = new StorageCredentials(sasToken);
            var cloudBlob = new CloudBlockBlob(destinationUri, credentials);

            try
            {
                cloudBlob.BeginUploadFromStream(stream,
                                                AccessCondition.GenerateEmptyCondition(),
                                                new BlobRequestOptions
                                                    {
                                                        RetryPolicy =
                                                            new ExponentialRetry(TimeSpan.FromSeconds(3), 5)
                                                    },
                                                null,
                                                BlockBlobUploadComplete,
                                                cloudBlob);
            }
            catch (StorageException ex)
            {
                // TODO: dude - how about some error handling?
                Trace.TraceError(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Infer from Uri version
        /// </summary>
        public static void UploadFile(string valetKeyUrl, string sourceFilePath)
        {
            UploadFile(new Uri(valetKeyUrl), sourceFilePath);
        }

        /// <summary>
        /// Given a Valet Key which allows writing/creating blobs for a Blob Container,
        /// write the given file into the equivalently-named blob.
        /// Currently ignores situations where the source filename is not allowed as the blob name.
        /// Name in blob storage is based on sourceFilePath and is placed in the Container referenced
        /// by valetKeyUrl.
        /// </summary>
        public static void UploadFile(Uri valetKeyUri, string sourceFilePath)
        {
            var desinationUri = BuildDesinationUri(valetKeyUri.AbsoluteUri, sourceFilePath);

            string acceptHeaderValue = null;
            if (sourceFilePath.Contains("https://api.github.com"))
            {
                // horrible, horrible hack for the github webhook handler
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
                UploadStream(valetKeyUri, desinationUri, stream);
            }
        }
    }
}
