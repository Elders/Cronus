using System.Threading.Tasks;

namespace Elders.Cronus.EventStore
{
    public interface IEventStore
    {
        Task AppendAsync(AggregateCommit aggregateCommit);
        Task AppendAsync(AggregateCommitRaw aggregateCommitRaw);
        Task<EventStream> LoadAsync(IAggregateRootId aggregateId);
    }

    public interface IEventStore<TSettings> : IEventStore
        where TSettings : class
    {
    }
}
