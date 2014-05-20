using System;
using System.Linq;
using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Config;

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
    }
}