using System.Threading.Tasks;

namespace Elders.Cronus.EventStore
{
    public interface IEventStore
    {
        Task AppendAsync(AggregateCommit aggregateCommit);
        Task AppendAsync(AggregateEventRaw aggregateCommitRaw);
        Task<EventStream> LoadAsync(IBlobId aggregateId);
    }

    public interface IEventStore<TSettings> : IEventStore
        where TSettings : class
    {
    }
}
