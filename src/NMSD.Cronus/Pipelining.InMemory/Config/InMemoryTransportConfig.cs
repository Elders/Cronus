using System;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Hosting;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Pipelining.Transport.Config;

namespace NMSD.Cronus.Pipelining.InMemory.Config
{
    public static class InMemoryTransportConfig
    {
        public static PipelineConsumerSettings<T> InMemory<T>(this PipelineConsumerSettings<T> consumer, Action<IPipelineTransportSettings<T>> transportConfigure = null)
            where T : IEndpointConsumer<IMessage>
        {
            consumer.Transport = new InMemoryTransportSettings<T>(consumer.Transport.PipelineSettings);
            if (transportConfigure != null)
                transportConfigure(consumer.Transport);
            consumer.Transport.Build();
            return consumer;
        }

        public static PipelinePublisherSettings<T> InMemory<T>(this PipelinePublisherSettings<T> publisher, Action<IPipelineTransportSettings<T>> transportConfigure = null)
            where T : IPublisher
        {
            publisher.Transport = new InMemoryTransportSettings<T>(publisher.Transport.PipelineSettings);
            if (transportConfigure != null)
                transportConfigure(publisher.Transport);
            publisher.Transport.Build();
            return publisher;
        }

        public static PipelineEventStoreConsumerSettings<T> InMemory<T>(this PipelineEventStoreConsumerSettings<T> esConsumer, Action<IPipelineTransportSettings<T>> transportConfigure = null)
            where T : EndpointEventStoreConsumer
        {
            esConsumer.Transport = new InMemoryTransportSettings<T>(esConsumer.Transport.PipelineSettings);
            if (transportConfigure != null)
                transportConfigure(esConsumer.Transport);
            esConsumer.Transport.Build();
            return esConsumer;
        }
    }
}
