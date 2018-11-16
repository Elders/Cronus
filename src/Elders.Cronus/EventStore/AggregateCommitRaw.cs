namespace Elders.Cronus.EventStore
{
    /// <summary>
    /// Represents an <see cref="AggregateCommit"/> in a raw form. This means that you need to take care
    /// about the deserialization in order to work with the data. This <see cref="AggregateCommitRaw"/> is
    /// really usefull in a situation where you want to copy/move the data without touching it.
    /// </summary>
    public sealed class AggregateCommitRaw
    {
        public AggregateCommitRaw(string aggregateRootId, byte[] data)
        {
            AggregateRootId = aggregateRootId;
            Data = data;
        }

        public string AggregateRootId { get; private set; }

        public byte[] Data { get; private set; }
    }
}
