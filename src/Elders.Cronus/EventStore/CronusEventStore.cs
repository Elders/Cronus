using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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

        public async Task AppendAsync(AggregateCommit aggregateCommit)
        {
            AggregateCommit transformedAggregateCommit = aggregateCommit;
            try
            {
                transformedAggregateCommit = aggregateCommitTransformer.Transform(aggregateCommit);
                await eventStore.AppendAsync(transformedAggregateCommit).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => $"Failed to append aggregate with id = {transformedAggregateCommit.AggregateRootId}. \n Exception: {ex.Message}");
                throw;
            }
        }

        public async Task AppendAsync(AggregateCommitRaw aggregateCommitRaw)
        {
            try
            {
                await eventStore.AppendAsync(aggregateCommitRaw).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => $"Failed to append aggregate with id = {aggregateCommitRaw.AggregateRootId}. \n Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<EventStream> LoadAsync(AggregateRootId aggregateId)
        {
            try
            {
                return await eventStore.LoadAsync(aggregateId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => $"Failed to load aggregate with id = {aggregateId}. \n Exception: {ex.Message}");
                throw;
            }
        }
    }
}
