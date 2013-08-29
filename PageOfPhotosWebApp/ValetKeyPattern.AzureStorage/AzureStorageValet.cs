using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValetKeyPattern.AzureStorage
{
   /// <summary>
   /// Intended for use a base class to Queue, Table, Blob, and BlobContainer classes
   /// </summary>
   public abstract class AzureStorageValet
   {
      public Uri ValetKeyUri { get; protected set; }

      protected AzureStorageValet(string valetKeyUrl) : this(new Uri(valetKeyUrl))
      {}

      protected AzureStorageValet(Uri valetKeyUri)
      {
         ValetKeyUri = valetKeyUri;
      }

      private const string UrlSchemeDefault = "https";

      internal bool IsValidValetUrl(string url)
      {
         return IsValidValetUri(new Uri(url));
      }

      internal bool IsValidValetUri(Uri uri)
      {
         // TODO: add more tests
         return uri.Query.Length > 1;
      }

      internal bool IsValidValetKey(string valetKeyUrl)
      {
         return IsValidValetKey(new Uri(valetKeyUrl));
      }

      /// <summary>
      /// Use override in derived class if you can do a better job proving validity specific to Blob, Container, Queue, or Table
      /// </summary>
      /// <param name="valetKeyUri"></param>
      /// <returns></returns>
      internal virtual bool IsValidValetKey(Uri valetKeyUri)
      {
         return !String.IsNullOrEmpty(valetKeyUri.Query);
      }

      internal void EnsureValidValetKey(string valetKeyUrl)
      {
         EnsureValidValetKey(new Uri(valetKeyUrl));
      }

      internal void EnsureValidValetKey(Uri valetKeyUri)
      {
         if (!IsValidValetKey(valetKeyUri))
         {
            throw new ArgumentException("URL pointing to Cloud Storage is expected to include Query String containing Shared Access Signature (VKP)", "valetKeyUri");
         }

         // TODO: add more checks
      }

      /// <summary>
      /// Throws appropriate exception if it can't ensure
      /// </summary>
      /// <param name="valetKeyUri"></param>
      internal void EnsureValidLocalStorageUri(Uri valetKeyUri)
      {
         var isHttps = (valetKeyUri.Scheme == Uri.UriSchemeHttps);
         if (isHttps) throw new ArgumentException("URL pointing to Local Storage cannot use HTTPS", "valetKeyUri");
      }

      internal Uri BuildDestinationUri(string valetKeyUrl, string sourceFilePath)
      {
         // FAILS (drops path on first param): return new Uri(GetDestinationPath(valetKeyUrl), GetFileNameFromUrl(sourceFilePath));
         // SUCCEEDS: return new Uri(String.Format("{0}/{1}", GetDestinationPath(valetKeyUrl), GetFileNameFromUrl(sourceFilePath));
         return new Uri(String.Format("{0}/{1}", GetDestinationPath(valetKeyUrl), sourceFilePath.FileName()));
      }

      internal Uri GetDestinationPath(string valetKeyUrl)
      {
         var valetKeyUri = new Uri(valetKeyUrl);
         var hostUri = new Uri(String.Format("{0}://{1}", valetKeyUri.Scheme, valetKeyUri.Host));
         return new Uri(hostUri, valetKeyUri.LocalPath);
      }

      public string SasToken { get { return ValetKeyUri.Query; } }
   }
}
