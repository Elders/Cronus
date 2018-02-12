namespace Elders.Cronus.Projections.Snapshotting
{
    public class NoSnapshot : ISnapshot
    {
        public NoSnapshot(IBlobId id, string projectionContractId)
        {
            Id = id;
            ProjectionContractId = projectionContractId;
        }

        public IBlobId Id { get; set; }

        public string ProjectionContractId { get; set; }

        public int Revision { get { return 0; } }

        public object State { get; private set; }

        public void InitializeState(object state) { }
    }
}
