using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "55f9e248-7bb3-4288-8db8-ba9620c67228")]
    public class EventToAggregateRootId : IEventStoreIndex
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(EventToAggregateRootId));

        private readonly IIndexStore indexStore;

        public EventToAggregateRootId(IIndexStore indexStore)
        {
            this.indexStore = indexStore;
        }

        public async Task IndexAsync(AggregateCommit aggregateCommit)
        {
            List<IndexRecord> indexRecordsBatch = new List<IndexRecord>();
            foreach (var @event in aggregateCommit.Events)
            {
                string eventTypeId = @event.Unwrap().GetType().GetContractId();
                var record = new IndexRecord(eventTypeId, aggregateCommit.AggregateRootId);
                indexRecordsBatch.Add(record);
            }

            foreach (var publicEvent in aggregateCommit.PublicEvents)
            {
                string eventTypeId = publicEvent.GetType().GetContractId();
                var record = new IndexRecord(eventTypeId, aggregateCommit.AggregateRootId);
                indexRecordsBatch.Add(record);
            }

            await indexStore.ApendAsync(indexRecordsBatch).ConfigureAwait(false);
        }

        public async Task IndexAsync(CronusMessage message)
        {
            var @event = message.Payload as IEvent;
            string eventTypeId = @event.Unwrap().GetType().GetContractId();

            var indexRecord = new List<IndexRecord>();
            indexRecord.Add(new IndexRecord(eventTypeId, Encoding.UTF8.GetBytes(message.GetRootId())));
            await indexStore.ApendAsync(indexRecord).ConfigureAwait(false);
        }

        public IAsyncEnumerable<IndexRecord> EnumerateRecordsAsync(string dataId)
        {
            // TODO: index exists?
            return indexStore.GetAsync(dataId);
        }

        public Task<LoadIndexRecordsResult> EnumerateRecordsAsync(string dataId, string paginationToken, int pageSize = 5000)
        {
            // TODO: index exists?
            return indexStore.GetAsync(dataId, paginationToken, pageSize);
        }
    }
}
