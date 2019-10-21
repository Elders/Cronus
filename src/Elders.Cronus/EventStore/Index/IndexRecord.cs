namespace Elders.Cronus.EventStore.Index
{
    public class IndexRecord
    {
        public IndexRecord(string id, byte[] aggregateRootId)
        {
            Id = id;
            AggregateRootId = aggregateRootId;
        }

        public string Id { get; private set; }

        public byte[] AggregateRootId { get; private set; }
    }
}
