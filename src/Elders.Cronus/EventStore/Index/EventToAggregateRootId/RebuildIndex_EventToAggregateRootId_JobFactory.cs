using Elders.Cronus.Cluster.Job;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.EventStore.Index;

public class RebuildIndex_EventToAggregateRootId_JobFactory : IRebuildIndex_EventToAggregateRootId_JobFactory
{
    private readonly RebuildIndex_EventToAggregateRootId_Job job;
    private readonly IJobNameBuilder jobNameBuilder;

    public RebuildIndex_EventToAggregateRootId_JobFactory(RebuildIndex_EventToAggregateRootId_Job job, IJobNameBuilder jobNameBuilder)
    {
        this.job = job;
        this.jobNameBuilder = jobNameBuilder;
    }

    public ICronusJob<object> CreateJob(VersionRequestTimebox timebox, int maxDegreeOfParallelism)
    {
        job.Name = jobNameBuilder.GetJobName(job.Name);
        job.BuildInitialData(() => new RebuildIndex_JobData()
        {
            Timestamp = timebox.RequestStartAt,
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        });

        return job;
    }

    public string GetJobName()
    {
        return jobNameBuilder.GetJobName(job.Name);
    }
}
