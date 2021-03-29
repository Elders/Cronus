using Elders.Cronus.Projections.Cassandra.EventSourcing;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "f8c532eb-57ad-469f-9002-6c286bdd88f2")]
    public class MessageCounterIndex : IEventStoreIndex
    {
        private readonly IMessageCounter eventCounter;

        public MessageCounterIndex(IMessageCounter eventCounter)
        {
            this.eventCounter = eventCounter;
        }

        public void Index(CronusMessage message)
        {
            if (message.Payload is IEvent @event)
            {
                eventCounter.Increment(@event.Unwrap().GetType());
            }
        }
    }
}
