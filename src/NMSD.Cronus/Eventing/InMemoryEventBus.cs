using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Eventing
{
    public class InMemoryEventBus : InMemoryBus<IEvent, IMessageHandler>, IPublisher<DomainMessageCommit>
    {
        private readonly IAggregateRepository eventStore;
      //  SqlConnection connection;

        public InMemoryEventBus(IAggregateRepository eventStore)
        {
            this.eventStore = eventStore;
        }

        public bool Publish(DomainMessageCommit message)
        {
            //if (connection == null || connection.State != System.Data.ConnectionState.Open)
            //    connection = eventStore.OpenConnection();
            //try
            //{
            //    eventStore.Persist(message.Events, connection);
            //    eventStore.TakeSnapshot(new List<IAggregateRootState>() { message.State }, connection);

            //    foreach (var @event in message.Events)
            //    {
            //        Publish(@event);
            //    }
            //    return true;
            //}
            //catch (Exception)
            //{
            //    eventStore.CloseConnection(connection);
            //    throw;
            //}
            return false;
        }
    }
}
