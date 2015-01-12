using System.IO;

namespace Elders.Cronus.Serializer
{
    public interface ISerializer
    {
        object Deserialize(Stream str);
        void Serialize<T>(Stream str, T message);
    }

    public static class ISerializerExtensions
    {
        public static object DeserializeFromBytes(this ISerializer self, byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return self.Deserialize(stream);
            }
        }

        public static byte[] SerializeToBytes<T>(this ISerializer self, T message)
        {
            using (var stream = new MemoryStream())
            {
                self.Serialize(stream, message);
                stream.Position = 0;
                return stream.ToArray();
            }
        }
    }
}
