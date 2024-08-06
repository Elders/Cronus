namespace Elders.Cronus.Projections.PartitionIndex
{
    public class ProjectionPartition
    {
        public ProjectionPartition(string projectionName, byte[] projectionId, long partition)
        {
            ProjectionName = projectionName;
            ProjectionId = projectionId;
            Partition = partition;
        }

        public string ProjectionName { get; private set; }

        public byte[] ProjectionId { get; private set; }

        public long Partition { get; private set; }
    }
}
