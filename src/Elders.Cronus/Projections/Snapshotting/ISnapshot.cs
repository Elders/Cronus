namespace Elders.Cronus.Projections.Snapshotting
{
    public interface ISnapshot
    {
        IBlobId Id { get; }
        int Revision { get; }
        object State { get; }
        string ProjectionName { get; }
    }
}
