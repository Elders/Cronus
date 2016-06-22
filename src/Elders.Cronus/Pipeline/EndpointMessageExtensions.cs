using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline
{
    public static class EndpointMessageExtensions
    {
        public static EndpointMessage AsEndpointMessage(this IMessage message, ISerializer serializer, string routingKey = "", Dictionary<string, object> routingHeaders = null)
        {
            CronusMessage transportMessage = new CronusMessage(new Message(message));

            byte[] body = serializer.SerializeToBytes(transportMessage);
            Dictionary<string, object> headers = routingHeaders ?? new Dictionary<string, object>() { { MessageInfo.GetContractId(transportMessage.Payload.Payload.GetType()), String.Empty } };
            EndpointMessage endpointMessage = new EndpointMessage(body, routingKey, headers);
            return endpointMessage;
        }

        public static EndpointMessage AsEndpointMessage(this CronusMessage message, ISerializer serializer, string routingKey = "", Dictionary<string, object> routingHeaders = null)
        {
            CronusMessage transportMessage = new CronusMessage(message);

            byte[] body = serializer.SerializeToBytes(transportMessage);
            Dictionary<string, object> headers = routingHeaders ?? new Dictionary<string, object>() { { MessageInfo.GetContractId(message.Payload.Payload.GetType()), String.Empty } };
            EndpointMessage endpointMessage = new EndpointMessage(body, routingKey, headers);
            return endpointMessage;
        }
    }
}
