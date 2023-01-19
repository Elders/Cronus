using System;
using Elders.Cronus.Cluster.Job;

namespace Elders.Cronus.EventStore.Index
{
    public class RebuildIndex_JobData : IJobData
    {
        public bool IsCompleted { get; set; } = false;

        public string PaginationToken { get; set; }

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
