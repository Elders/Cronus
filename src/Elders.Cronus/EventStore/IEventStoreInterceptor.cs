namespace Elders.Cronus.EventStore
{
    public interface IAggregateInterceptor
    {
        AggregateCommit Transform(AggregateCommit origin);
    }

    public class EmptyAggregateTransformer : IAggregateInterceptor
    {
        public AggregateCommit Transform(AggregateCommit origin) => origin;
    }
}
