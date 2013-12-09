using System.Collections.Generic;
using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Core.Snapshotting
{
    public class SnapShotter : ISnapShotter
    {
        public void TakeSnapshot(List<IAggregateRootState> state)
        {

        }
    }
}