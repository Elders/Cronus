using System.Collections.Generic;
using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Core.Snapshotting
{
    public interface ISnapShotter
    {
        void TakeSnapshot(List<IAggregateRootState> state);
    }
}