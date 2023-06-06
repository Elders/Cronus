using Elders.Cronus.EventStore.Players;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;

namespace Elders.Cronus.Projections.Rebuilding
{
    public class Projection_JobFactory
    {
        private readonly RebuildProjection_Job job;
        private readonly CronusContext context;
        private readonly BoundedContext boundedContext;

        public Projection_JobFactory(RebuildProjection_Job job, IOptions<BoundedContext> boundedContext, CronusContext context)
        {
            this.job = job;
            this.context = context;
            this.boundedContext = boundedContext.Value;
        }

        public RebuildProjection_Job CreateJob(ProjectionVersion version, ReplayEventsOptions replayEventsOptions, VersionRequestTimebox timebox)
        {
            job.Name = $"urn:{boundedContext.Name}:{context.Tenant}:{job.Name}:{version.ProjectionName}_{version.Hash}_{version.Revision}";

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
}
