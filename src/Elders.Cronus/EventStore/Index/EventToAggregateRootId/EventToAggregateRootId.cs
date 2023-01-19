using Elders.Cronus.Projections.Cassandra.EventSourcing;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "3d59f948-870f-4b12-ada6-9603627aaab6")]
    public class EventToAggregateRootId : ICronusEventStoreIndex
    {
        private readonly IIndexStore indexStore;

        public EventToAggregateRootId() { }

        public EventToAggregateRootId(IIndexStore indexStore)
        {
            this.indexStore = indexStore;
        }

        public Task IndexAsync(CronusMessage message)
        {
            IEvent @event = message.Payload as IEvent;
            string eventTypeId = @event.Unwrap().GetType().GetContractId();

            var record = new IndexRecord(eventTypeId, Encoding.UTF8.GetBytes(message.GetRootId()), message.GetRevision(), message.GetRootEventPosition(), message.GetTimestamp());

            return indexStore.ApendAsync(record);
        }
    }
}
