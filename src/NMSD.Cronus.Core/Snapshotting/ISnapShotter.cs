using System.Collections.Generic;
using System.Data.SqlClient;
using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Core.Snapshotting
{
    public interface ISnapShotter
    {
        void TakeSnapshot(List<IAggregateRootState> state, SqlConnection connection);
    }
}