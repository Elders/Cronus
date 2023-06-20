using Elders.Cronus.EventStore.Index;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore
{
    public interface IEventStore
    {
        Task AppendAsync(AggregateCommit aggregateCommit);
        Task AppendAsync(AggregateEventRaw eventRaw);
        Task<EventStream> LoadAsync(IBlobId aggregateId);
        Task<bool> DeleteAsync(AggregateEventRaw eventRaw);
        Task<LoadAggregateRawEventsWithPagingResult> LoadWithPagingDescendingAsync(IBlobId aggregateId, PagingOptions pagingOptions);
        Task<AggregateEventRaw> LoadAggregateEventRaw(IndexRecord indexRecord);
    }

    public interface IEventStore<TSettings> : IEventStore
        where TSettings : class
    {
    }
}
