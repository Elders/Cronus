using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;
using Elders.Cronus.Logging;

namespace Elders.Cronus.Pipeline
{
    public class PipelinePublisher<T> : Publisher<T>, IDisposable
        where T : IMessage
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(PipelinePublisher<T>));

        private readonly IPipelineTransport transport;

        private readonly ISerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelinePublisher{T}"/> class.
        /// </summary>
        /// <param name="transport">The transport.</param>
        /// <param name="serializer">The serializer.</param>
        public PipelinePublisher(IPipelineTransport transport, ISerializer serializer)
        {
            this.transport = transport;
            this.serializer = serializer;
        }

        protected override bool PublishInternal(T message, Dictionary<string, string> messageHeaders)
        {
            var payload = new Message(message, messageHeaders);
            var transportMessage = new TransportMessage(payload);

            byte[] body = serializer.SerializeToBytes(transportMessage);
            Dictionary<string, object> routingHeaders = new Dictionary<string, object>() { { transportMessage.Payload.Payload.GetType().GetContractId(), String.Empty } };
            EndpointMessage endpointMessage = new EndpointMessage(body, string.Empty, routingHeaders);

            transport.PipelineFactory
                .GetPipeline(message.GetType())
                .Push(endpointMessage);
            return true;
        }

        public void Dispose()
        {
            transport.Dispose();
        }
    }
}
