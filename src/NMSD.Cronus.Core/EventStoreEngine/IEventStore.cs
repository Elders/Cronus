using System.Collections.Generic;
using System.Data.SqlClient;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Cqrs;

namespace Cronus.Core.EventStore
{
    /// <summary>
    /// Indicates the ability to store and retreive a stream of events.
    /// </summary>
    /// <remarks>
    /// Instances of this class must be designed to be multi-thread safe such that they can be shared between threads.
    /// </remarks>
    public interface IEventStore
    {
        AR Load<AR>(IAggregateRootId aggregateId) where AR : IAggregateRoot;

        void Save(IAggregateRoot aggregateRoot);

       // IAggregateRootState LoadAggregateState(System.Guid aggregateId);

        //void Persist(List<IEvent> events, SqlConnection connection);

        //void TakeSnapshot(List<IAggregateRootState> states, SqlConnection connection);

        //SqlConnection OpenConnection();

        //void CloseConnection(SqlConnection conn);
    }
}
