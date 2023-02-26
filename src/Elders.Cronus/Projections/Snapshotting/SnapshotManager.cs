namespace Elders.Cronus.Projections.Snapshotting
{
    public class SnapshotManager : AggregateRoot<SnapshotState>
    {
        public SnapshotManager() { }
        public SnapshotManager(SnapshotId id, ISnapshot snapshot)
        {
            SnapshotCreated @event = new SnapshotCreated(id, snapshot);
            Apply(@event);
        }

        public void CreateSnapshot(SnapshotId id, ISnapshot snapshot)
        {
            SnapshotCreated @event = new SnapshotCreated(id, snapshot);
            Apply(@event);
        }
    }

    public class SnapshotState : AggregateRootState<SnapshotManager, SnapshotId>
    {
        public override SnapshotId Id { get; set; }
        public ISnapshot Snapshot { get; set; }

        public void When(SnapshotCreated e)
        {
            Id = e.SnapshotId;
            Snapshot = e.Snapshot;
        }
    }
}
