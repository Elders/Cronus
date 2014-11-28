using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline
{
    public class PipelinePublisher<T> : Publisher<T>, IDisposable
        where T : IMessage
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PipelinePublisher<T>));

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

        protected override bool PublishInternal(T message)
        {
            transport.PipelineFactory
                .GetPipeline(message.GetType())
                .Push(message.AsEndpointMessage(serializer));
            return true;
        }

        public void Dispose()
        {
            transport.Dispose();
        }
    }
}