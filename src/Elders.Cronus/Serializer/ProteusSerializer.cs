using System.Linq;
using System.Reflection;
using Elders.Cronus.DomainModeling;
using Elders.Proteus;

namespace Elders.Cronus.Serializer.Protoreg
{
    public class ProteusSerializer : ISerializer
    {
        Proteus.Serializer serializer;

        public ProteusSerializer(Assembly[] assembliesContainingContracts)
        {
            var internalAssemblies = assembliesContainingContracts.ToList();
            internalAssemblies.Add(typeof(IAggregateRoot).Assembly);
            internalAssemblies.Add(typeof(CronusAssembly).Assembly);
            internalAssemblies.Add(typeof(Proteus.Serializer).Assembly);

            var identifier = new GuidTypeIdentifier(assembliesContainingContracts);
            serializer = new Proteus.Serializer(identifier);
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