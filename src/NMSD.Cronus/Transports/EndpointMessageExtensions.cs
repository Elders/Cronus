using System.IO;
using NMSD.Cronus.Messaging;
using NMSD.Protoreg;

namespace NMSD.Cronus.Transports
{
    public static class EndpointMessageExtensions
    {
        public static EndpointMessage AsEndpointMessage(this IMessage message, ProtoregSerializer serializer)
        {
            byte[] body;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, message);
                body = stream.ToArray();
            }
            return new EndpointMessage(body);
        }
    }
}
