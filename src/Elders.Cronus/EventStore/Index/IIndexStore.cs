using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.EventStore.Index
{
    public interface IIndexStore
    {
        void Apend(IEnumerable<IndexRecord> indexRecords);
        IEnumerable<IndexRecord> Get(string indexRecordId);
        LoadIndexRecordsResult Get(string indexRecordId, string paginationToken, int pageSize);
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
