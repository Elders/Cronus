using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.EventStore.Index;

namespace Elders.Cronus.EventStore.InMemory
{
    public class InMemoryIndexStore : IIndexStore
    {
        private ConcurrentDictionary<string, List<IndexRecord>> indexRecords = new ConcurrentDictionary<string, List<IndexRecord>>();

        public void Apend(IEnumerable<IndexRecord> indexRecords)
        {
            foreach (var record in indexRecords)
            {
                if (this.indexRecords.ContainsKey(record.Id) == false)
                    this.indexRecords.TryAdd(record.Id, new List<IndexRecord>());

                this.indexRecords[record.Id].Add(record);
            }
        }

        public IEnumerable<IndexRecord> Get(string indexRecordId)
        {
            if (indexRecords.ContainsKey(indexRecordId))
                return indexRecords[indexRecordId];

            return Enumerable.Empty<IndexRecord>();
        }
    }
}
