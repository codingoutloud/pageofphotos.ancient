using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;

namespace ValetKeyPattern.AzureStorage
{
   /// <summary>
   /// Intended for use a base class to Queue, Table, Blob, and BlobContainer classes
   /// </summary>
   public abstract class AzureStorageValet
   {
      public Uri ValetKeyUri { get; protected set; }

      protected AzureStorageValet(string valetKeyUrl)
         : this(new Uri(valetKeyUrl))
      { }

      protected AzureStorageValet(Uri valetKeyUri)
      {
         ValetKeyUri = valetKeyUri;
      }

      internal bool IsEmulated()
      {
         return ValetKeyUri.IsEmulated();
      }

      public static bool IsValidValetKeyUrl(string url)
      {
         if (String.IsNullOrWhiteSpace(url)) return false;
         return IsValidValetKeyUri(new Uri(url));
      }

      public static bool IsValidValetKeyUri(Uri uri)
      {
         if (uri.Query.Length < 1) return false;

         var sas = uri.Query;
         if (sas.Contains("%2B")) return false; // should probably be "+"
         if (sas.Contains("%2F")) return false; // should probably be "/"
         if (sas.Contains("%3A")) return false; // should probably be ":"
         if (sas.Contains("%3D")) return false; // should probably be "="

         return true;
      }

      protected abstract Uri GetSpecificBaseUri();

      private const string UrlSchemeDefault = "https";


      internal bool IsValidValetUrl(string url)// TODO: deprecate
      {
         return IsValidValetUri(new Uri(url));
      }

      internal bool IsValidValetUri(Uri uri)//TODO: deprecate
      {
         // TODO: add more tests
         return uri.Query.Length > 1;
      }

      internal bool IsValidValetKey(string valetKeyUrl) //TODO: deprecate
      {
         return IsValidValetKey(new Uri(valetKeyUrl));
      }

      /// <summary>
      /// Use override in derived class if you can do a better job proving validity specific to Blob, Container, Queue, or Table
      /// </summary>
      /// <param name="valetKeyUri"></param>
      /// <returns></returns>
      internal virtual bool IsValidValetKey(Uri valetKeyUri) // TODO: deprecate
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
         return GetDestinationPath(valetKeyUri);
      }
      internal Uri GetDestinationPath(Uri valetKeyUri)
      {
         var hostUri = new Uri(String.Format("{0}://{1}", valetKeyUri.Scheme, valetKeyUri.Host));
         return new Uri(hostUri, valetKeyUri.LocalPath);
      }

      private string SasToken { get { return ValetKeyUri.Query; } }

      public Uri BaseUri
      {
         get
         {
            var aa = GetSpecificBaseUri();
            var xx = ValetKeyUri.BaseUri();
            var _x = new CloudQueueClient(xx, StorageCredentials);
            var _y = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudQueueClient();

            if (ValetKeyUri.IsEmulated())
            {
               return GetSpecificBaseUri();
            }
            else
            {
               return ValetKeyUri.BaseUri();
            }
         }
      }

      public StorageCredentials StorageCredentials
      {
         get
         {
            if (ValetKeyUri.IsEmulated())
            {
               return CloudStorageAccount.DevelopmentStorageAccount.Credentials;
            }
            else
            {
               return new StorageCredentials(SasToken);
            }
         }
      }
   }
}
