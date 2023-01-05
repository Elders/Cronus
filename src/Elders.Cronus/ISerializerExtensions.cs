using CommunityToolkit.HighPerformance;
using System;
using System.IO;
using System.Text;

namespace Elders.Cronus
{
    public static class ISerializerExtensions
    {
        public static object DeserializeFromBytes(this ISerializer self, byte[] bytes)
        {
            Encoding.UTF8.GetString(bytes);
            using (var stream = new MemoryStream(bytes))
            {
                return self.Deserialize(stream);
            }
        }

        public static object DeserializeFromBytes(this ISerializer self, ReadOnlyMemory<byte> bytes)
        {
            using (var stream = bytes.AsStream())
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
