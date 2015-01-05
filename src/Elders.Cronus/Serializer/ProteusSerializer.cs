using System.Reflection;
using Elders.Proteus;

namespace Elders.Cronus.Serializer.Protoreg
{
    public class ProteusSerializer : ISerializer
    {
        Proteus.Serializer serializer;

        public ProteusSerializer()
        {
            serializer = new Proteus.Serializer();
        }

        public ProteusSerializer(Assembly[] assembliesContainingContracts)
        {
            if (assembliesContainingContracts == null)
            {
                serializer = new Proteus.Serializer();
            }
            else
            {
                var identifier = new GuidTypeIdentifier(assembliesContainingContracts);
                serializer = new Proteus.Serializer(identifier);
            }
        }

        public object Deserialize(System.IO.Stream str)
        {
            return serializer.DeserializeWithHeaders(str);
        }

        public void Serialize<T>(System.IO.Stream str, T message)
        {
            serializer.SerializeWithHeaders(str, message);
        }
    }
}