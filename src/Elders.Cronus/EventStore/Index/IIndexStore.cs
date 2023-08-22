using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    public interface IIndexStore
    {
        Task ApendAsync(IndexRecord indexRecord);
        IAsyncEnumerable<IndexRecord> GetAsync(string indexRecordId);
        Task<LoadIndexRecordsResult> GetAsync(string indexRecordId, string paginationToken, int pageSize);

        Task<long> GetCountAsync(string indexRecordId);
    }

    public class LoadIndexRecordsResult
    {
        public LoadIndexRecordsResult()
        {
            Records = new List<IndexRecord>();
        }

        public string PaginationToken { get; set; }

        public IEnumerable<IndexRecord> Records { get; set; }

        public static LoadIndexRecordsResult Empty(string paginationToken)
        {
            return new LoadIndexRecordsResult()
            {
                Records = Enumerable.Empty<IndexRecord>(),
                PaginationToken = paginationToken
            };
        }
    }
}
