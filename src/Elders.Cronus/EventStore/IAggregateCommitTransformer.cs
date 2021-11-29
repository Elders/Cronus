namespace Elders.Cronus.EventStore
{
    public interface IAggregateCommitTransformer
    {
        AggregateCommit Transform(AggregateCommit origin);
    }

    public class NoAggregateCommitTransformer : IAggregateCommitTransformer
    {
        public AggregateCommit Transform(AggregateCommit origin) => origin;
    }
}
