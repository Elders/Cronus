using System.Collections.Generic;

namespace Elders.Cronus.EventStore.Index
{
    public interface IIndexStore
    {
        void Apend(IEnumerable<IndexRecord> indexRecords);
        IEnumerable<IndexRecord> Get(string indexRecordId);
    }
}
