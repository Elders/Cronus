namespace Elders.Cronus.Snapshots.SnapshotStore
{
    public record Snapshot(IBlobId Id, int Revision, object State);
}
