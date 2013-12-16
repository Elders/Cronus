using System;
using System.Collections.Generic;
using Cronus.Core.Eventing;
using Cronus.Core.EventStore;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Messaging;

namespace NMSD.Cronus.Core.Eventing
{
    public class InMemoryEventBus : InMemoryBus<IEvent, IMessageHandler>, IPublisher<MessageCommit>
    {
        private readonly IEventStore eventStore;

        public InMemoryEventBus(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public bool Publish(MessageCommit message)
        {
            var connection = eventStore.OpenConnection();
            try
            {
                eventStore.Persist(message.Events, connection);
                eventStore.TakeSnapshot(new List<IAggregateRootState>() { message.State }, connection);

                foreach (var @event in message.Events)
                {
                    Publish(@event);
                }
                return true;
            }
            catch (Exception ex)
            {
                eventStore.CloseConnection(connection);
                throw ex;
            }
        }
    }
}
