namespace Elders.Cronus.Projections.Snapshotting
{
    public class Snapshot : ISnapshot
    {
        public Snapshot(IBlobId id, string projectionContractId, object state, int revision)
        {
            Id = id;
            ProjectionContractId = projectionContractId;
            State = state;
            Revision = revision;
        }

        public IBlobId Id { get; private set; }

        public string ProjectionContractId { get; private set; }

        public object State { get; set; }

        public int Revision { get; private set; }

        public void InitializeState(object state)
        {
            State = state;
        }
    }
}
