using Elders.Cronus.Projections.Cassandra.EventSourcing;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "f8c532eb-57ad-469f-9002-6c286bdd88f2")]
    public class MessageCounterIndex : ICronusEventStoreIndex
    {
        private readonly IMessageCounter eventCounter;

        public MessageCounterIndex(IMessageCounter eventCounter)
        {
            this.eventCounter = eventCounter;
        }

        public Task IndexAsync(CronusMessage message)
        {
            if (message.Payload is IEvent @event)
            {
                return eventCounter.IncrementAsync(@event.Unwrap().GetType());
            }

            return Task.CompletedTask;
        }
    }
}
