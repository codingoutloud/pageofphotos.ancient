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
         using (var ms = new MemoryStream())
         {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, m);
            return ms.ToArray();
         }
      }

      public static T Deserialize(byte[] byteArray)
      {
         using (var ms = new MemoryStream(byteArray))
         {
            var formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(ms);
         }
      }
   }
}
