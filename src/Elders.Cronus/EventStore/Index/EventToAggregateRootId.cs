using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

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

        public void Index(AggregateCommit aggregateCommit)
        {
            List<IndexRecord> indexRecordsBatch = new List<IndexRecord>();
            foreach (var @event in aggregateCommit.Events)
            {
                string eventTypeId = @event.Unwrap().GetType().GetContractId();
                var record = new IndexRecord(eventTypeId, aggregateCommit.AggregateRootId);
                indexRecordsBatch.Add(record);
            }

            indexStore.Apend(indexRecordsBatch);
        }

        public void Index(CronusMessage message)
        {
            var @event = message.Payload as IEvent;
            string eventTypeId = @event.Unwrap().GetType().GetContractId();

            var indexRecord = new List<IndexRecord>();
            indexRecord.Add(new IndexRecord(eventTypeId, Encoding.UTF8.GetBytes(message.GetRootId())));
            indexStore.Apend(indexRecord);
        }

        public IEnumerable<IndexRecord> EnumerateRecords(string dataId)
        {
            // TODO: index exists?
            return indexStore.Get(dataId);
        }
    }
}
