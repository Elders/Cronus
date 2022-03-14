using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Options;

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

        public RebuildProjection_Job CreateJob(ProjectionVersion version, VersionRequestTimebox timebox)
        {
            job.Name = $"urn:{boundedContext.Name}:{context.Tenant}:{job.Name}:{version.ProjectionName}_{version.Hash}_{version.Revision}";

            job.BuildInitialData(() =>
            {
                var data = new RebuildProjection_JobData();
                data.Timestamp = timebox.RequestStartAt;
                data.DueDate = timebox.FinishRequestUntil;
                data.Version = version;

                return data;
            });

            return job;
        }
    }
}
