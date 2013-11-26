using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Core.Snapshotting
{
    public interface ISnapShotter
    {
        void TakeSnapshot(IAggregateRootState state);
    }
}