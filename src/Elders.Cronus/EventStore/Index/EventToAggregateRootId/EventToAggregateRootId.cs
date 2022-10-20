using Elders.Cronus.Projections.Cassandra.EventSourcing;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "3d59f948-870f-4b12-ada6-9603627aaab6")]
    public class EventToAggregateRootId : ICronusEventStoreIndex, IEventStoreJobIndex
    {
        private readonly IIndexStore indexStore;

        public EventToAggregateRootId() { }

        public EventToAggregateRootId(IIndexStore indexStore)
        {
            this.indexStore = indexStore;
        }

        public async Task IndexAsync(AggregateCommit aggregateCommit)
        {
            int possition = -1;
            for (var eventPosition = 0; eventPosition < aggregateCommit.Events.Count; eventPosition++)
            {
                string eventTypeId = aggregateCommit.Events[eventPosition].Unwrap().GetType().GetContractId();
                var record = new IndexRecord(eventTypeId, aggregateCommit.AggregateRootId, aggregateCommit.Revision, ++possition, aggregateCommit.Timestamp);
                await indexStore.ApendAsync(record).ConfigureAwait(false);
            }

            possition += 5;
            foreach (IPublicEvent publicEvent in aggregateCommit.PublicEvents)
            {
                string eventTypeId = publicEvent.GetType().GetContractId();
                var record = new IndexRecord(eventTypeId, aggregateCommit.AggregateRootId, aggregateCommit.Revision, possition++, aggregateCommit.Timestamp);
                await indexStore.ApendAsync(record).ConfigureAwait(false);
            }
        }

        public Task IndexAsync(CronusMessage message)
        {
            IEvent @event = message.Payload as IEvent;
            string eventTypeId = @event.Unwrap().GetType().GetContractId();

            var record = new IndexRecord(eventTypeId, Encoding.UTF8.GetBytes(message.GetRootId()), message.GetRevision(), message.GetRootEventPosition(), message.GetTimestamp());

            return indexStore.ApendAsync(record);
        }

        public IAsyncEnumerable<IndexRecord> EnumerateRecords(string dataId)
        {
            // TODO: index exists?
            return indexStore.GetAsync(dataId);
        }

        public Task<LoadIndexRecordsResult> EnumerateRecordsAsync(string dataId, string paginationToken, int pageSize = 5000)
        {
            // TODO: index exists?
            return indexStore.GetAsync(dataId, paginationToken, pageSize);
        }

        //public IAsyncEnumerable<LoadIndexRecordsResult> EnumerateRecordsAsync(string dataId, string paginationToken, int pageSize = 5000)
        //{
        //    // TODO: index exists?
        //    return indexStore.GetRecordsAsync(dataId, paginationToken, pageSize);
        //}
    }
}
