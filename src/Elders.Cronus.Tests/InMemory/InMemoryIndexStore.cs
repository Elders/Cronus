//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Elders.Cronus.EventStore.Index;

//namespace Elders.Cronus.EventStore.InMemory
//{
//    public class InMemoryIndexStore : IIndexStore
//    {
//        private ConcurrentDictionary<string, List<IndexRecord>> indexRecords = new ConcurrentDictionary<string, List<IndexRecord>>();

//        public Task ApendAsync(IEnumerable<IndexRecord> indexRecords)
//        {
//            foreach (var record in indexRecords)
//            {
//                if (this.indexRecords.ContainsKey(record.Id) == false)
//                    this.indexRecords.TryAdd(record.Id, new List<IndexRecord>());

//                this.indexRecords[record.Id].Add(record);
//            }

//            return Task.CompletedTask;
//        }

//        public Task<IEnumerable<IndexRecord>> GetAsync(string indexRecordId)
//        {
//            if (indexRecords.ContainsKey(indexRecordId))
//                return Task.FromResult((IEnumerable<IndexRecord>)indexRecords[indexRecordId]);

//            return Task.FromResult(Enumerable.Empty<IndexRecord>());
//        }

//        public Task<LoadIndexRecordsResult> GetAsync(string indexRecordId, string paginationToken, int pageSize)
//        {
//            if (indexRecords.ContainsKey(indexRecordId))
//            {
//                var result = new LoadIndexRecordsResult()
//                {
//                    PaginationToken = paginationToken,
//                    Records = indexRecords[indexRecordId]
//                };

//                return Task.FromResult(result);
//            }

//            return Task.FromResult(LoadIndexRecordsResult.Empty(paginationToken));
//        }
//    }
//}
