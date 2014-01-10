using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.DomainModelling;

namespace NMSD.Cronus.Core.Eventing
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
