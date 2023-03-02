using System;
using Elders.Cronus.Cluster.Job;

namespace Elders.Cronus.Snapshots
{
    public sealed class CreateSnapshot_JobData : IJobData
    {
        public bool IsCompleted { get; set; } = false;
        public DateTimeOffset Timestamp { get; set; }
        public Urn Id { get; set; }
        public int Revision { get; set; }
        public string AggregateContract { get; set; }
        public string Error { get; set; }
    }
}
