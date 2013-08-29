using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

// ReSharper disable CheckNamespace
namespace DevPartners.Azure
// ReSharper restore CheckNamespace
{
   public static class ByteArraySerializer<T>
   {
      public static byte[] Serialize(T m)
      {
         var ms = new MemoryStream();
         try
         {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, m);
            return ms.ToArray();
         }
         finally
         {
            ms.Close();
         }
      }

      public static T Deserialize(byte[] byteArray)
      {
         var ms = new MemoryStream(byteArray);
         try
         {
            var formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(ms);
         }
         finally
         {
            ms.Close();
         }
      }
   }
}
