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
   public class AzureStorageValet
   {
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

   }
}
