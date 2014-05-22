using System;
using System.Collections.Generic;
using System.IO;
using Elders.Cronus.DomainModelling;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline
{
    public static class EndpointMessageExtensions
    {
        public static EndpointMessage AsEndpointMessage(this IMessage message, ProtoregSerializer serializer, string routingKey = "", Dictionary<string, object> routingHeaders = null)
        {
            TransportMessage transportMessage = new TransportMessage(message);

            byte[] body;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, transportMessage);
                body = stream.ToArray();
            }
            Dictionary<string, object> headers = routingHeaders ?? new Dictionary<string, object>() { { MessageInfo.GetContractId(transportMessage.Payload.GetType()), String.Empty } };
            EndpointMessage endpointMessage = new EndpointMessage(body, routingKey, headers);
            return endpointMessage;
        }

        public static EndpointMessage AsEndpointMessage(this TransportMessage message, ProtoregSerializer serializer, string routingKey = "", Dictionary<string, object> routingHeaders = null)
        {
            TransportMessage transportMessage = new TransportMessage(message);

            byte[] body;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, transportMessage);
                body = stream.ToArray();
            }
            Dictionary<string, object> headers = routingHeaders ?? new Dictionary<string, object>() { { MessageInfo.GetContractId(message.Payload.GetType()), String.Empty } };
            EndpointMessage endpointMessage = new EndpointMessage(body, routingKey, headers);
            return endpointMessage;
        }

        public static EndpointMessage AsErrorEndpointMessage(this IMessage message, ProtoregSerializer serializer, string routingKey = "", Dictionary<string, object> routingHeaders = null)
        {
            byte[] body;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, message);
                body = stream.ToArray();
            }
            Dictionary<string, object> headers = new Dictionary<string, object>();
            EndpointMessage endpointMessage = new EndpointMessage(body, routingKey, headers);
            return endpointMessage;
        }
    }
}
