using System;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore;
using Elders.Cronus.IntegrityValidation;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Snapshots
{
    public sealed class CreateSnapshot_Job : CronusJob<CreateSnapshot_JobData>
    {
        private readonly IEventStore eventStore;
        private readonly ISnapshotWriter snapshotWriter;
        private readonly IIntegrityPolicy<EventStream> integrityPolicy;

        public CreateSnapshot_Job(IEventStore eventStore, ISnapshotWriter snapshotWriter, IIntegrityPolicy<EventStream> integrityPolicy, ILogger<CronusJob<CreateSnapshot_JobData>> logger) : base(logger)
        {
            this.eventStore = eventStore;
            this.snapshotWriter = snapshotWriter;
            this.integrityPolicy = integrityPolicy;
        }

        public override string Name { get; set; }

        protected override async Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            try
            {
                EventStream eventStream = await eventStore.LoadAsync(Data.Id).ConfigureAwait(false);

                var integrityResult = integrityPolicy.Apply(eventStream);
                if (integrityResult.IsIntegrityViolated)
                {
                    logger.Error(() => "Aggregate root's integrity with id {id} is violated.", Data.Id);
                    Data.Error = $"Aggregate root's integrity with id {Data.Id} is violated.";
                    Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);
                    return JobExecutionStatus.Failed;
                }

                eventStream = integrityResult.Output;
                var arType = Data.AggregateContract.GetTypeByContract();
                if (eventStream.TryRestoreFromHistory(Data.Revision, arType, out var aggregateRoot) == false)
                {
                    logger.Error(() => "Unable to restore aggregate root with id {id} from history.", Data.Id);
                    Data.Error = $"Unable to restore aggregate root with id {Data.Id} from history.";
                    Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);
                    return JobExecutionStatus.Failed;
                }

                if (aggregateRoot is IHaveState<IAggregateRootState> iHaveState)
                {
                    await snapshotWriter.WriteAsync(Data.Id, Data.Revision, iHaveState.State).ConfigureAwait(false);
                    Data.Error = null;
                    Data.IsCompleted = true;
                    Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

                    return JobExecutionStatus.Completed;
                }

                Data.Error = $"Aggregate root does not implement {nameof(IHaveState<IAggregateRootState>)}. Canceling...";
                Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);
                return JobExecutionStatus.Canceled;
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => "{jobName} job failed.", nameof(CreateSnapshot_Job));
                Data.Error = ex.Message;
                Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

                return JobExecutionStatus.Failed;
            }
        }
    }
}
