using System.Collections.Generic;
using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Core.Snapshotting
{
    public class SnapShotter : IPersistEventStream
    {
        public void TakeSnapshot(List<IAggregateRootState> state)
        {

        }

        public void TakeSnapshot(List<IAggregateRootState> state, System.Data.SqlClient.SqlConnection connection)
        {
            throw new System.NotImplementedException();
        }

        public void Persist(List<Eventing.IEvent> events, System.Data.SqlClient.SqlConnection connection)
        {
            throw new System.NotImplementedException();
        }

        public System.Data.SqlClient.SqlConnection OpenConnection()
        {
            throw new System.NotImplementedException();
        }

        public void CloseConnection(System.Data.SqlClient.SqlConnection conn)
        {
            throw new System.NotImplementedException();
        }

        public IEventStream OpenStream()
        {
            throw new System.NotImplementedException();
        }

        public void Commit(IEventStream stream)
        {
            throw new System.NotImplementedException();
        }
    }
}