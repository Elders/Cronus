namespace Elders.Cronus.EventStore
{
    /// <summary>
    /// Represents an <see cref="AggregateCommit"/> in a raw form. This means that you need to take care
    /// about the deserialization in order to work with the data. This <see cref="AggregateEventRaw"/> is
    /// really usefull in a situation where you want to copy/move the data without touching it.
    /// </summary>
    public sealed class AggregateEventRaw
    {
        public AggregateEventRaw(byte[] aggregateRootId, byte[] data, int revision, int position, long timestamp)
        {
            AggregateRootId = aggregateRootId;
            Data = data;
            Revision = revision;
            Position = position;
            Timestamp = timestamp;
        }

        public byte[] AggregateRootId { get; private set; }

        public byte[] Data { get; private set; }

        public int Revision { get; private set; }

        public int Position { get; private set; }

        public long Timestamp { get; private set; }

    }
}
