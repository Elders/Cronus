using Elders.Cronus.Cluster.Job;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.EventStore.Index
{
    public class RebuildNewIndex_EventToAggregateRootId_JobFactory : IRebuildIndex_EventToAggregateRootId_JobFactory
    {
        private readonly RebuildNewIndex_EventToAggregateRootId_Job job;
        private readonly IJobNameBuilder jobNameBuilder;

        public RebuildNewIndex_EventToAggregateRootId_JobFactory(RebuildNewIndex_EventToAggregateRootId_Job job, IJobNameBuilder jobNameBuilder)
        {
            this.job = job;
            this.jobNameBuilder = jobNameBuilder;
        }

        public ICronusJob<object> CreateJob(VersionRequestTimebox timebox)
        {
            job.Name = jobNameBuilder.GetJobName(job.Name);
            job.BuildInitialData(() => new RebuildIndex_JobData()
            {
                Timestamp = timebox.RequestStartAt
            });

            return job;
        }

        public string GetJobName()
        {
            return jobNameBuilder.GetJobName(job.Name);
        }
    }
}
