using Microsoft.WindowsAzure.Storage.Auth;

namespace WindowsAzureStorageCredentialsSwizzler
{
    public static class WindowsAzureStorageCredentialsExtensions
    {
       public static StorageCredentials Echo(this StorageCredentials creds)
       {
          return creds;
       }
    }
}
