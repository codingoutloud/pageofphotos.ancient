using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace WindowsAzureStorageCredentialsSwizzler
{
   public static class StorageCredentialsSwizzler
   {
      #region internal helpers

      internal static bool AddressingCloudStorage(string url)
      {
         return AddressingCloudStorage(new Uri(url));
      }

      /// <summary>
      /// Ascertain whether the URL passed is pointing to Cloud Storage or Local Storage. Harder than one might think!
      /// </summary>
      /// <param name="uri"></param>
      /// <returns>true iff the URL is determined to be addressing Cloud Storage</returns>
      internal static bool AddressingCloudStorage(Uri uri)
      {
         var isHttps = (uri.Scheme == Uri.UriSchemeHttps);
         var hasCloudStorageIndicator = uri.Host.EndsWith(".core.windows.net");
         
         if (isHttps && !hasCloudStorageIndicator) throw new ArgumentException("URL pointing to Local Storage cannot use HTTPS", "url");

         return hasCloudStorageIndicator;
      }

      /// <summary>
      /// Throws appropriate exception if it can't ensure
      /// </summary>
      /// <param name="valetKeyUri"></param>
      internal static void EnsureValidValetKey(Uri valetKeyUri)
      {
         if (String.IsNullOrEmpty(valetKeyUri.Query)) throw new ArgumentException("URL pointing to Cloud Storage is expected to include Query String containing Shared Access Signature (VKP)", "valetKeyUri");
      }

      /// <summary>
      /// Throws appropriate exception if it can't ensure
      /// </summary>
      /// <param name="valetKeyUri"></param>
      internal static void EnsureValidLocalStorageUri(Uri valetKeyUri)
      {
         var isHttps = (valetKeyUri.Scheme == Uri.UriSchemeHttps);
         if (isHttps) throw new ArgumentException("URL pointing to Local Storage cannot use HTTPS", "valetKeyUri");
      }
      #endregion

      /// <summary>
      /// Important reference: http://msdn.microsoft.com/en-us/library/windowsazure/hh403989.aspx
      /// </summary>
      /// <param name="url">URL string holding -EITHER- a full Shared Access Signature URL (following Valet Key Pattern) -OR- local connection (like http://127.0.0.1:10000/account/path) </param>
      /// <returns>StorageCredentials object, or throws Exception</returns>
      public static StorageCredentials CreateFromUrl(string url)
      {
         return CreateFromUri(new Uri(url));
      }

      /// <summary>
      /// Important reference: http://msdn.microsoft.com/en-us/library/windowsazure/hh403989.aspx
      /// </summary>
      /// <param name="valetKeyUri">URL string holding -EITHER- a full Shared Access Signature URL (following Valet Key Pattern) -OR- local connection (like http://127.0.0.1:10000/account/path) </param>
      /// <returns>StorageCredentials object, or throws Exception</returns>
      public static StorageCredentials CreateFromUri(Uri valetKeyUri)
      {
         if (AddressingCloudStorage(valetKeyUri))
         {
            EnsureValidValetKey(valetKeyUri);
            var creds = new StorageCredentials(valetKeyUri.Query);
            return creds;
         }
         else
         {
            EnsureValidLocalStorageUri(valetKeyUri);
            return CloudStorageAccount.DevelopmentStorageAccount.Credentials;
         }
      }

      public static string QueueName(string valetKeyUrl)
      {
         return QueueName(new Uri(valetKeyUrl));
      }

      /// <summary>
      /// TODO: Currently does not check that it is really a Queue
      /// </summary>
      /// <param name="valetKeyUri"></param>
      /// <returns></returns>
      public static string QueueName(Uri valetKeyUri)
      {
         if (AddressingCloudStorage(valetKeyUri))
         {
            return valetKeyUri.AbsolutePath.Substring(1); // Substring(1) removes leading "/"
         }
         else
         {
            return valetKeyUri.AbsolutePath.Split('/')[2]; // assumes valetKeyUri.AbsolutePath == "/accountname/queuename"
         }
      }

      public static Uri QueueBaseUri(Uri valetKeyUri, bool forceHttps = false)
      {
         var scheme = valetKeyUri.Scheme;
         if (forceHttps)
         {
            scheme = Uri.UriSchemeHttps;
         }
         return new Uri(String.Format("{0}://{1}", scheme, valetKeyUri.Host));
      }
   }
}
