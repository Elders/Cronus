namespace Elders.Cronus.EventStore
{
    public interface IEventStoreInterceptor
    {
        AggregateCommit Transform(AggregateCommit origin);
    }

    public class NoAggregateCommitTransformer : IEventStoreInterceptor
    {
        public AggregateCommit Transform(AggregateCommit origin) => origin;
    }
}
