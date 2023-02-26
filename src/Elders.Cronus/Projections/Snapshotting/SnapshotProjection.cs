using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections.Snapshotting
{
    [DataContract(Name = "d40e8cb2-4b25-48c5-b9ea-f020417c59b8")]
    public class SnapshotProjection : ProjectionDefinition<SnapshotProjectionState, IBlobId>, IAmNotSnapshotable, ISystemProjection, INonVersionableProjection,
        IEventHandler<SnapshotCreated>
    {
        public SnapshotProjection()
        {
            Subscribe<SnapshotCreated>(x => x.SnapshotId);
        }

        public Task HandleAsync(SnapshotCreated @event)
        {
            State.SnapshotId = @event.SnapshotId;
            State.Snapshot = @event.Snapshot;

            return Task.CompletedTask;
        }
    }

    public class SnapshotProjectionState
    {
        public IBlobId SnapshotId { get; set; }

        public ISnapshot Snapshot { get; set; }
    }
}
