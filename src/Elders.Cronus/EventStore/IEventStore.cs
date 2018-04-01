namespace Elders.Cronus.EventStore
{
    public interface IEventStore
    {
        void Append(AggregateCommit aggregateCommit);
        EventStream Load(IAggregateRootId aggregateId);
    }
}
