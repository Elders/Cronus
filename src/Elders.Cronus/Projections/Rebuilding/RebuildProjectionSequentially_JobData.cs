using System;
using Elders.Cronus.Cluster.Job;

namespace Elders.Cronus.Projections.Rebuilding
{
    public class RebuildProjectionSequentially_JobData : IJobData
    {
        public RebuildProjectionSequentially_JobData()
        {
            IsCompleted = false;
            IsCanceled = false;
            Timestamp = DateTimeOffset.UtcNow;
            DueDate = DateTimeOffset.MaxValue;
        }

        public bool IsCompleted { get; set; }
        public bool IsCanceled { get; set; }
        public string PaginationToken { get; set; }
        public ulong ProcessedCount { get; set; }
        public ulong TotalCount { get; set; }
        public ProjectionVersion Version { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public DateTimeOffset? After { get; set; }
        public DateTimeOffset? Before { get; set; }
        public int MaxDegreeOfParallelism { get; set; }
    }
}
