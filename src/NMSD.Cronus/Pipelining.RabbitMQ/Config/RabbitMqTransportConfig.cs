using System;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Hosting;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Pipelining.Transport.Config;

namespace NMSD.Cronus.Pipelining.RabbitMQ.Config
{
    public static class RabbitMqTransportConfig
    {
        public static PipelineConsumerSettings<T> RabbitMq<T>(this PipelineConsumerSettings<T> consumer, Action<IPipelineTransportSettings<T>> transportConfigure = null)
            where T : IEndpointConsumer<IMessage>
        {
            consumer.Transport = new RabbitMqTransportSettings<T>(consumer.Transport.PipelineSettings);
            if (transportConfigure != null)
                transportConfigure(consumer.Transport);
            consumer.Transport.Build();
            return consumer;
        }

        public static PipelinePublisherSettings<T> RabbitMq<T>(this PipelinePublisherSettings<T> publisher, Action<IPipelineTransportSettings<T>> transportConfigure = null)
            where T : IPublisher
        {
            publisher.Transport = new RabbitMqTransportSettings<T>(publisher.Transport.PipelineSettings);
            if (transportConfigure != null)
                transportConfigure(publisher.Transport);
            publisher.Transport.Build();
            return publisher;
        }

        public static PipelineEventStoreConsumerSettings<T> RabbitMq<T>(this PipelineEventStoreConsumerSettings<T> esConsumer, Action<IPipelineTransportSettings<T>> transportConfigure = null)
            where T : EndpointEventStoreConsumer
        {
            esConsumer.Transport = new RabbitMqTransportSettings<T>(esConsumer.Transport.PipelineSettings);
            if (transportConfigure != null)
                transportConfigure(esConsumer.Transport);
            esConsumer.Transport.Build();
            return esConsumer;
        }
    }
}
