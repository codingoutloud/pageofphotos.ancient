using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace ValetKeyPattern.AzureStorage
{
   public static class ValetKeyUriParser
   {
      /// <summary>
      /// Infer from IsEmulated(Uri uri)
      /// </summary>
      internal static bool IsEmulated(this string url)
      {
         return new Uri(url).IsEmulated();
      }

      /// <summary>
      /// Ascertain whether the URL passed is pointing to Cloud Storage or Local Storage.
      /// Though I don't think the name is perfect, chose "IsEmulated" to match "RoleEnvironment.IsEmulated" from Azure SDK to
      /// make it more familiar and obvious.
      /// </summary>
      /// <param name="uri"></param>
      /// <returns>true iff the URL is determined to be addressing Cloud Storage</returns>
      internal static bool IsEmulated(this Uri uri)
      {
         var isHttps = (uri.Scheme == Uri.UriSchemeHttps);
         var hasCloudStorageIndicator = uri.Host.EndsWith(".core.windows.net");

         if (isHttps && !hasCloudStorageIndicator) throw new ArgumentException("Cannot use HTTPS when pointing to Local Storage", "uri");

         Contract.Assert(hasCloudStorageIndicator != uri.IsLoopback); // TODO: move to a unit test

         var isEmulated = !hasCloudStorageIndicator;
         return isEmulated;
      }

      public static string PathNoQuery(this Uri uri)
      {
         var path = uri.ToString();
         var queryStart = path.IndexOf("?", StringComparison.InvariantCulture);

         return queryStart > 0 ? path.Substring(0, queryStart) : path;
      }

      /// <summary>
      /// Blob Container name, Queue name, or Table name
      /// </summary>
      /// <param name="uri"></param>
      /// <returns></returns>
      internal static string StoragePartitionName(this Uri uri)
      {
         if (IsEmulated(uri))
         {
            // assumes valetKeyUri.AbsolutePath == "/accountname/queuename" 
            // (so, in emulator, storage account is in first segment)
            var partition = uri.Segments[2].Replace("/", ""); // 
            return partition;
         }
         else
         {
            var partition = uri.Segments[1].Replace("/", "");
            return partition;
         }
      }

      internal static Uri BaseUri(this string valetKeyUrl, bool forceHttps = false)
      {
         return new Uri(valetKeyUrl).BaseUri(forceHttps);
      }

      /// <summary>
      /// </summary>
      /// <param name="valetKeyUri"></param>
      /// <param name="forceHttps">Ignored when addressing Local Storage</param>
      /// <returns></returns>
      internal static Uri BaseUri(this Uri valetKeyUri, bool forceHttps = false)
      {
         Contract.Requires(valetKeyUri != null);

         if (ValetKeyUriParser.IsEmulated(valetKeyUri))
         {  // Local Storage (DevFabric)
            Contract.Assert(valetKeyUri.IsLoopback);
            Contract.Assert(valetKeyUri.Port != 80 && valetKeyUri.Port != 443);
            Contract.Assert(valetKeyUri.Scheme != Uri.UriSchemeHttps);
            return new Uri(String.Format("{0}://{1}:{2}/{3}", Uri.UriSchemeHttp, valetKeyUri.Host, valetKeyUri.Port, Constants.CloudEmulatorStorageAccountName));
         }
         else
         {  // Cloud Storage
            Contract.Assert(valetKeyUri.Port == 80 || valetKeyUri.Port == 443);
            var scheme = valetKeyUri.Scheme;
            if (forceHttps)
            {
               scheme = Uri.UriSchemeHttps;
            }
            return new Uri(String.Format("{0}://{1}", scheme, valetKeyUri.Host));
         }
      }

       /// <summary>
       /// Infer from FileName(Uri uri)
       /// </summary>
       public static string FileName(this string url)
       {
           return new Uri(url).FileName();
       }

      /// <summary>
      /// From c:\x\y\foo.txt, return foo.txt
      /// From http://example.com/x/y/foo.txt, return foo.txt
      /// </summary>
      /// <param name="uri">Source URL of file name. A URL or local file path.</param>
      /// <returns></returns>
      public static string FileName(this Uri uri)
      {
          var filePath = uri.LocalPath;
          var fileInfo = new FileInfo(filePath);
          return fileInfo.Name;
      }
   }
}

