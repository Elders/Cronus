using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore.Players;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.Projections.Rebuilding;

public class RebuildProjection_JobFactory : IProjection_JobFactory
{
    private readonly RebuildProjection_Job job;
    private readonly ICronusContextAccessor contextAccessor;
    private readonly BoundedContext boundedContext;

    public RebuildProjection_JobFactory(RebuildProjection_Job job, IOptions<BoundedContext> boundedContext, ICronusContextAccessor contextAccessor)
    {
        this.job = job;
        this.contextAccessor = contextAccessor;
        this.boundedContext = boundedContext.Value;
    }

    public ICronusJob<object> CreateJob(ProjectionVersion version, ReplayEventsOptions replayEventsOptions, VersionRequestTimebox timebox)
    {
        job.Name = $"urn:{contextAccessor.CronusContext.Tenant}:{boundedContext.Name}:{job.Name}:{version.ProjectionName}_{version.Hash}_{version.Revision}";

        job.BuildInitialData(() => new RebuildProjection_JobData()
        {
            After = replayEventsOptions.After,
            Before = replayEventsOptions.Before,
            MaxDegreeOfParallelism = replayEventsOptions.MaxDegreeOfParallelism,
            Timestamp = timebox.RequestStartAt,
            DueDate = timebox.FinishRequestUntil,
            Version = version

        });

        return job;
    }
}
