using System;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.Snapshots
{
    public sealed class CreateSnapshot_JobFactory
    {
        private readonly CreateSnapshot_Job job;
        private readonly CronusContext context;
        private readonly BoundedContext boundedContext;

        public CreateSnapshot_JobFactory(CreateSnapshot_Job job, IOptions<BoundedContext> boundedContext, CronusContext context)
        {
            this.job = job;
            this.context = context;
            this.boundedContext = boundedContext.Value;
        }

        public CreateSnapshot_Job CreateJob(Urn id, string aggregateContract, int revision, DateTimeOffset requestedOn)
        {
            job.Name = $"urn:{boundedContext.Name}:{context.Tenant}:{id}_{revision}";

            job.BuildInitialData(() =>
            {
                var data = new CreateSnapshot_JobData();
                data.Timestamp = requestedOn;
                data.Id = id;
                data.Revision = revision;
                data.AggregateContract = aggregateContract;

                return data;
            });

            return job;
        }
    }
}
