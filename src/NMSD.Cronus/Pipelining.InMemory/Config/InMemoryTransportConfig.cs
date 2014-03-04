using System;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Hosting;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Pipelining.Transport.Config;

namespace NMSD.Cronus.Pipelining.InMemory.Config
{
    public static class InMemoryTransportConfig
    {
        public static PipelineConsumerSettings<T> InMemory<T>(this PipelineConsumerSettings<T> consumer, Action<IPipelineTransportSettings<T>> transportConfigure)
            where T : IStartableConsumer<IMessage>
        {
            consumer.Transport = new InMemoryTransportSettings<T>();
            transportConfigure(consumer.Transport);
            consumer.Transport.Build();
            return consumer;
        }

        public static PipelinePublisherSettings<T> InMemory<T>(this PipelinePublisherSettings<T> publisher, Action<IPipelineTransportSettings<T>> transportConfigure)
            where T : IPublisher
        {
            publisher.Transport = new InMemoryTransportSettings<T>();
            transportConfigure(publisher.Transport);
            publisher.Transport.Build();
            return publisher;
        }
    }
}
