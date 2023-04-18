using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.Snapshots.SnapshotStore;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Snapshots.Job
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
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                EventStream eventStream = await eventStore.LoadAsync(Data.Id).ConfigureAwait(false);
                logger.Info(() => "Loaded event stream for requested snapshot with id {id} for {elapsed}.", Data.Id, sw.Elapsed);

                var integrityResult = integrityPolicy.Apply(eventStream);
                if (integrityResult.IsIntegrityViolated)
                {
                    sw.Stop();
                    var elapsed = sw.Elapsed;
                    logger.Error(() => "Aggregate root's integrity with id {id} is violated, time elapsed: {elapsed}", Data.Id, elapsed);
                    Data.Error = $"Aggregate root's integrity with id {Data.Id} is violated, time elapsed: {elapsed}.";
                    Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);
                    return JobExecutionStatus.Failed;
                }

                eventStream = integrityResult.Output;
                var arType = Data.Contract.GetTypeByContract();
                if (eventStream.TryRestoreFromHistory(Data.Revision, arType, out var aggregateRoot) == false)
                {
                    sw.Stop();
                    var elapsed = sw.Elapsed;
                    logger.Error(() => "Unable to restore aggregate root with id {id} from history for {elapsed}", Data.Id, elapsed);
                    Data.Error = $"Unable to restore aggregate root with id {Data.Id} from history for {elapsed}.";
                    Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);
                    return JobExecutionStatus.Failed;
                }

                if (aggregateRoot is IAggregateRoot root)
                {
                    if (root.IsSnapshotable())
                    {
                        var method = root.GetType().GetMethod(nameof(IAmSnapshotable<object>.CreateSnapshot), BindingFlags.Public | BindingFlags.Instance);
                        var snapshot = method.Invoke(root, null);
                        await snapshotWriter.WriteAsync(Data.Id, Data.Revision, snapshot).ConfigureAwait(false);
                        Data.Error = null;
                        Data.IsCompleted = true;

                        sw.Stop();
                        logger.Info(() => "Created snapshot for aggregate root with id {id} for {elapsed}.", Data.Id, sw.Elapsed);
                        Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);
                        return JobExecutionStatus.Completed;
                    }
                    else
                    {
                        sw.Stop();
                        Data.Error = $"Aggregate root state does not implement {nameof(IAmSnapshotable<object>)}. Canceling...";
                        Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);
                        return JobExecutionStatus.Canceled;
                    }
                }
                else
                {
                    sw.Stop();
                    Data.Error = $"Aggregate root does not implement {nameof(IAggregateRoot)}. Canceling...";
                    Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);
                    return JobExecutionStatus.Canceled;
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                logger.ErrorException(ex, () => "{jobName} job for aggregate root with id {id} failed, running for {elapsed}.", nameof(CreateSnapshot_Job), Data.Id, sw.Elapsed);
                Data.Error = ex.Message + Environment.NewLine + $"Running for {sw.Elapsed}.";
                Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

                return JobExecutionStatus.Failed;
            }
        }
    }
}
