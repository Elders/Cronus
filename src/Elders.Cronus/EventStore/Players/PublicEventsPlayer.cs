using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Elders.Cronus.Cluster.Job;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.EventStore.Players;

[DataContract(Name = "51b93c21-20fb-473f-b7fc-c12e6a56e194")]
public sealed class PublicEventsPlayer : ISystemTrigger,
    ISignalHandle<ReplayPublicEventsRequested>
{
    private readonly ICronusJobRunner jobRunner;
    private readonly ReplayPublicEvents_JobFactory jobFactory;
    private readonly ILogger<PublicEventsPlayer> logger;

    public PublicEventsPlayer(ICronusJobRunner jobRunner, ReplayPublicEvents_JobFactory jobFactory, ILogger<PublicEventsPlayer> logger)
    {
        this.jobRunner = jobRunner;
        this.jobFactory = jobFactory;
        this.logger = logger;
    }

    public async Task HandleAsync(ReplayPublicEventsRequested signal)
    {
        try
        {
            ReplayPublicEvents_Job job = jobFactory.CreateJob(signal);
            JobExecutionStatus result = await jobRunner.ExecuteAsync(job).ConfigureAwait(false);

            logger.Debug(() => "Replay public events finished.");
        }
        catch (Exception ex) when (logger.ErrorException(ex, () => "Failed to replay public events.")) { }
    }
}
