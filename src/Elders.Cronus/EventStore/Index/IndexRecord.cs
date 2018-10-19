using System.Collections.Generic;

namespace Elders.Cronus.EventStore.Index
{
    public interface IIndexStatusStore
    {
        void Save(string indexId, IndexStatus status);
        IndexStatus Get(string indexId);
    }

    public interface IIndexStore
    {
        void Apend(IEnumerable<IndexRecord> indexRecords);
        IEnumerable<IndexRecord> Get(string indexRecordId);
    }

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
