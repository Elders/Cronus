using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Failed to append aggregate with id = {transformedAggregateCommit.AggregateRootId}. \n Exception: {ex.Message}"))
            {
                throw;
            }
        }

        public async Task AppendAsync(AggregateEventRaw aggregateCommitRaw)
        {
            try
            {
                await eventStore.AppendAsync(aggregateCommitRaw).ConfigureAwait(false);
            }
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Failed to append aggregate with id = {aggregateCommitRaw.AggregateRootId}. \n Exception: {ex.Message}"))
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(AggregateEventRaw eventRaw)
        {
            try
            {
                return await eventStore.DeleteAsync(eventRaw).ConfigureAwait(false);
            }
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Failed to delete aggregate event with id = {eventRaw.AggregateRootId}. \n Exception: {ex.Message}"))
            {
                throw;
            }
        }

        public async Task<EventStream> LoadAsync(IBlobId aggregateId)
        {
            try
            {
                return await eventStore.LoadAsync(aggregateId).ConfigureAwait(false);
            }
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Failed to load aggregate with id = {aggregateId}. \n Exception: {ex.Message}"))
            {
                throw;
            }
        }

        public async Task<EventStream> LoadAsync(IBlobId aggregateId, int revision)
        {
            try
            {
                return await eventStore.LoadAsync(aggregateId, revision).ConfigureAwait(false);
            }
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Failed to load aggregate with id = {aggregateId}. \n Exception: {ex.Message}"))
            {
                throw;
            }
        }
    }
}
