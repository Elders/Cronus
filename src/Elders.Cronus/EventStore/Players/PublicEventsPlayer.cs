using System.Runtime.Serialization;
using Elders.Cronus.Cluster.Job;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.EventStore.Players
{

    [DataContract(Name = "51b93c21-20fb-473f-b7fc-c12e6a56e194")]
    public class PublicEventsPlayer : ISystemSaga,
        IEventHandler<ReplayPublicEventsRequested>
    {
        private static ILogger logger = CronusLogger.CreateLogger(typeof(ProjectionBuilder));

        private readonly ICronusJobRunner jobRunner;
        private readonly ReplayPublicEvents_JobFactory jobFactory;

        public PublicEventsPlayer(ICronusJobRunner jobRunner, ReplayPublicEvents_JobFactory jobFactory)
        {
            this.jobRunner = jobRunner;
            this.jobFactory = jobFactory;
        }

        public void Handle(ReplayPublicEventsRequested signal)
        {
            ReplayPublicEvents_Job job = jobFactory.CreateJob(signal);
            JobExecutionStatus result = jobRunner.ExecuteAsync(job).GetAwaiter().GetResult();

            logger.Debug(() => "Rebuild projection version {@cronus_projection_rebuild}", result);
        }
    }
}
