namespace Elders.Cronus.Projections.Snapshotting
{
    public class Snapshot : ISnapshot
    {
        public Snapshot(IBlobId id, string projectionName, object state, int revision)
        {
            Id = id;
            ProjectionName = projectionName;
            State = state;
            Revision = revision;
        }

        public IBlobId Id { get; private set; }

        public string ProjectionName { get; private set; }

        public object State { get; set; }

        public int Revision { get; private set; }

        public void InitializeState(object state)
        {
            State = state;
        }
    }
}
