using System.IO;

namespace Elders.Cronus
{
    public interface ISerializer
    {
        object Deserialize(Stream str);
        void Serialize<T>(Stream str, T message);
    }
}
