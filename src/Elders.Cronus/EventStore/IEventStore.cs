using System.Threading.Tasks;

namespace Elders.Cronus.EventStore
{
    public interface IEventStore
    {
        Task AppendAsync(AggregateCommit aggregateCommit);
        Task AppendAsync(AggregateEventRaw eventRaw);
        Task<EventStream> LoadAsync(IBlobId aggregateId);
        Task<bool> DeleteAsync(AggregateEventRaw eventRaw);
    }

    public interface IEventStore<TSettings> : IEventStore
        where TSettings : class
    {
    }
}
