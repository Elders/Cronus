using System;
using System.Linq;
using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport.RabbitMQ.Config;

namespace Elders.Cronus.Persistence.MSSQL.Config
{
    public static class EventStoreExtensions
    {
        public static T UseEventStoreHandler<T>(this T self, Lazy<IEventStore> eventStore, Assembly domainEventsAssembly, Action<MessageProcessorWithSafeBatchSettings<ICommand>> configure = null)
            where T : IHaveMessageProcessor<DomainMessageCommit>
        {
            MessageProcessorWithSafeBatchSettings<ICommand> settings = new MessageProcessorWithSafeBatchSettings<ICommand>();
            if (configure != null)
                configure(settings);

            var castedSettings = settings as IMessageProcessorWithSafeBatchSettings<ICommand>;


            self.MessageHandlerProcessor = new Lazy<IMessageProcessor<DomainMessageCommit>>(() =>
            {
                var retryStrategy = new DefaultRetryStrategy<DomainMessageCommit>() as ISafeBatchRetryStrategy<DomainMessageCommit>;
                var handler = new EventStoreHandler(domainEventsAssembly.ExportedTypes.First(), new EventStoreSafeBatchContextFactory(retryStrategy, eventStore.Value.Persister), castedSettings.ConsumerBatchSize);
                return handler;
            });
            return self;
        }

        public static T UseDefaultMsSqlEventStoreHost<T>(this T self, string boundedContext, Type domainEventsAssembly)
            where T : ICronusSettings
        {
            self
                .UseEventStoreConsumable(boundedContext, consumable => consumable
                    .SetNumberOfConsumers(2)
                    .UseRabbitMqTransport()
                    .EventStoreConsumer(consumer => consumer
                        .SetConsumeSuccessStrategy(new EndpointPostConsumeStrategy.EventStorePublishEventsOnSuccessPersist((self as IHaveEventPublisher).EventPublisher.Value))
                        .UseEventStoreHandler((self as IHaveEventStores).EventStores[boundedContext], domainEventsAssembly.Assembly)));
            return self;
        }
    }
}