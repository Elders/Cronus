using System;

namespace Elders.Cronus.Cluster.Job
{
    public interface IJobData
    {
        bool IsCompleted { get; set; }

        ///// <summary>
        ///// Indicates if the job data has been created locally. If the job data has been downloaded from the cluster the value will be false.
        ///// </summary>
        //bool IsLocal { get; set; }

        DateTimeOffset Timestamp { get; set; }
    }
}
