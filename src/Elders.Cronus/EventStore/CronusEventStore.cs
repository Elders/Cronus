using Elders.Cronus.EventStore.Index;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore
{
    public class CronusEventStore : IEventStore
    {
        private readonly IEventStore eventStore;
        private readonly ILogger<CronusEventStore> logger;

        public CronusEventStore(IEventStore eventStore, ILogger<CronusEventStore> logger)
        {
            this.eventStore = eventStore;
            this.logger = logger;
        }

        public async Task AppendAsync(AggregateCommit aggregateCommit)
        {
            try
            {
                await eventStore.AppendAsync(aggregateCommit).ConfigureAwait(false);
            }
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Failed to append aggregate with id = {aggregateCommit.AggregateRootId}. \n Exception: {ex.Message}"))
            {
                throw;
            }
        }

        public async Task AppendAsync(AggregateEventRaw aggregateEventRaw)
        {
            try
            {
                await eventStore.AppendAsync(aggregateEventRaw).ConfigureAwait(false);
            }
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Failed to append aggregate with id = {aggregateEventRaw.AggregateRootId}. \n Exception: {ex.Message}"))
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

        public async Task<AggregateEventRaw> LoadAggregateEventRaw(IndexRecord indexRecord)
        {
            try
            {
                return await eventStore.LoadAggregateEventRaw(indexRecord).ConfigureAwait(false);
            }
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Failed to load aggregate event raw with id = {indexRecord.AggregateRootId}. \n Exception: {ex.Message}"))
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

        public async Task<LoadAggregateRawEventsWithPagingResult> LoadWithPagingDescendingAsync(IBlobId aggregateId, PagingOptions pagingOptions)
        {
            try
            {
                return await eventStore.LoadWithPagingDescendingAsync(aggregateId, pagingOptions).ConfigureAwait(false);
            }
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Failed to load aggregate with id = {aggregateId} and Paging options {pagingOptions}. \n Exception: {ex.Message}"))
            {
                throw;
            }
        }
    }
}
