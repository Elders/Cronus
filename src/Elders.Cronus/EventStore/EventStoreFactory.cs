using Microsoft.Extensions.Logging;

namespace Elders.Cronus.EventStore
{
    public interface IEventStoreFactory
    {
        IEventStore GetEventStore();
    }

    public sealed class EventStoreFactory
    {
        private readonly IEventStore eventStore;
        private readonly IAggregateCommitTransformer aggregateCommitTransformer;
        private readonly ILogger<CronusEventStore> logger;

        public EventStoreFactory(IEventStore eventStore, IAggregateCommitTransformer aggregateCommitTransformer, ILogger<CronusEventStore> logger)
        {
            this.eventStore = eventStore;
            this.aggregateCommitTransformer = aggregateCommitTransformer;
            this.logger = logger;
        }

        public IEventStore GetEventStore()
        {
            return new CronusEventStore(eventStore, aggregateCommitTransformer, logger);
        }
    }
}
