using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValetKeyPattern.AzureStorage
{
   // TODO: convert BLOB notions to more generic STORAGE notions

    /// <summary>
    /// References a specific Windows Azure Blob by absolute Uri.
    /// Container and BlobName properties added for help accessing Blob-interesting parts of the full URL.
    /// While Blob path separators can be configured, this class currently assumes always "/".
    /// Query strings are ignored (Windows Azure Blob storage also ignores them).
    /// A BlobUri object "is a" Uri object; all Uri methods are available.
    /// </summary>
    // TODO: implement separate BlobContainerUri class
    public static class UriValidator
    {
       public static int MinLengthAzureBlobContainerName = 3;

       public static bool UrlIsValidForWindowsAzureStorage(string url)
       {
          return UriIsValidForWindowsAzureStorage(new Uri(url));
       }

       public static bool UriIsValidForWindowsAzureStorage(Uri uri)
       {
          return IsWellFormedStorageUri(uri);
       }

       public static void EnsureUriIsValidForWindowsAzureStorage(string url)
       {
          EnsureUriIsValidForWindowsAzureStorage(new Uri(url));
       }

       public static void EnsureUriIsValidForWindowsAzureStorage(Uri uri)
       {
          if (!IsWellFormedStorageUri(uri)) throw new UriFormatException(String.Format("Valid Uri, but not valid to address Windows Azure Storage: {0}", uri.AbsolutePath));
       }

       public static string GetBlobName(Uri uri)
       {
          string pathAndQuery = uri.PathAndQuery; // everything to the right of domain name
          string blobNameAndQuery = pathAndQuery.Substring(uri.StoragePartitionName().Length + 2);
             // blob name is after container...
          // ... but don't include query string (if one is present) [note: query strings are ignored by Blob Storage]
          // [http://blogs.msdn.com/b/windowsazure/archive/2011/03/18/best-practices-for-the-windows-azure-content-delivery-network.aspx]
          // ["In blob storage origin, query strings are always ignored. In particular, shared access strings cannot be used to enable CDN access to a private container."]
          string blobName = blobNameAndQuery.IndexOf('?') > 0
             ? blobNameAndQuery.Substring(0, blobNameAndQuery.IndexOf('?'))
             : blobNameAndQuery;
          return blobName;
       }

        public static bool ContainerNameIsLongEnough(Uri uri)
        {
           return uri.StoragePartitionName().Length >= MinLengthAzureBlobContainerName;
        }

        public static bool IsWellFormedStorageUri(Uri uri)
        {
            if (!uri.IsAbsoluteUri) return false;
            // if (!uri.IsWellFormedUriString(uri.AbsoluteUri, UriKind.Absolute)) return false;
            if (!UriHasTwoPartPath(uri)) return false;
            if (!ContainerNameIsLongEnough(uri)) return false; // after checking UriHasTwoPartPath, else exception

            return true;
        }

        private static bool UriHasTwoPartPath(Uri uri)
        {
            string path = uri.PathAndQuery;
            if (String.IsNullOrEmpty(uri.PathAndQuery)) return false; // http://example.com
            if (uri.PathAndQuery == "/") return false; // just http://example.com/ is not sufficient, need http://example.com/container/blob
            // must contain two path separators; we just made sure the first char is a "/" - now make sure there's a second later
            int indexOfSecondPathSeparator = path.IndexOf('/', 1);
            bool atLeastTwoPathSeparators = indexOfSecondPathSeparator > 1; // can't be in either first *or* second char
            if (!atLeastTwoPathSeparators) return false;
            // ensure there's at least one more non-path char after the second path separator
            if ((path.Length <= indexOfSecondPathSeparator + 1) || (path[indexOfSecondPathSeparator + 1] == '/'))
                return false;
            return true; // passed all the torture tests
        }
    }
}