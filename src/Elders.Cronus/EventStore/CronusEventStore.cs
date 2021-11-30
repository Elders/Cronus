using Microsoft.Extensions.Logging;
using System;

namespace Elders.Cronus.EventStore
{
    public class CronusEventStore : IEventStore
    {
        private readonly IEventStore eventStore;
        private readonly IEventStoreInterceptor aggregateCommitTransformer;
        private readonly ILogger<CronusEventStore> logger;

        public CronusEventStore(IEventStore eventStore, IEventStoreInterceptor aggregateCommitTransformer, ILogger<CronusEventStore> logger)
        {
            this.eventStore = eventStore;
            this.aggregateCommitTransformer = aggregateCommitTransformer;
            this.logger = logger;
        }

        public void Append(AggregateCommit aggregateCommit)
        {
            AggregateCommit transformedAggregateCommit = aggregateCommit;
            try
            {
                transformedAggregateCommit = aggregateCommitTransformer.Transform(aggregateCommit);
                eventStore.Append(transformedAggregateCommit);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => $"Failed to append aggregate with id = {transformedAggregateCommit.AggregateRootId}. \n Exception: {ex.Message}");
                throw;
            }
        }

        public void Append(AggregateCommitRaw aggregateCommitRaw)
        {
            try
            {
                eventStore.Append(aggregateCommitRaw);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => $"Failed to append aggregate with id = {aggregateCommitRaw.AggregateRootId}. \n Exception: {ex.Message}");
                throw;
            }
        }

        public EventStream Load(IAggregateRootId aggregateId)
        {
            try
            {
                return eventStore.Load(aggregateId);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => $"Failed to load aggregate with id = {aggregateId}. \n Exception: {ex.Message}");
                throw;
            }
        }
    }
}
