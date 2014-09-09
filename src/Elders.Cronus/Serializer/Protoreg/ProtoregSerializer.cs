using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.Serializer.Protoreg
{
    public class ProtoregSerializer : ISerializer
    {
        Elders.Protoreg.ProtoregSerializer serializer;

        public ProtoregSerializer(Elders.Protoreg.ProtoRegistration registration)
        {
            serializer = new Elders.Protoreg.ProtoregSerializer(registration);
        }

        public object Deserialize(System.IO.Stream str)
        {
            return serializer.Deserialize(str);
        }

        public void Serialize<T>(System.IO.Stream str, T message)
        {
            serializer.Serialize(str, message);
        }

        public void Build()
        {
            serializer.Build();
        }
    }
}
