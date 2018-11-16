namespace Elders.Cronus.EventStore
{
    public interface IEventStore
    {
        void Append(AggregateCommit aggregateCommit);
        void Append(AggregateCommitRaw aggregateCommitRaw);
        EventStream Load(IAggregateRootId aggregateId);
    }

    public interface IEventStore<TSettings> : IEventStore
        where TSettings : class
    {
    }
}
