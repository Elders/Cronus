namespace Elders.Cronus.EventStore.Index
{
    public interface IAggregateCommitHandle<in T>
        where T : AggregateCommit
    {
        void Handle(T @event);
    }
}
