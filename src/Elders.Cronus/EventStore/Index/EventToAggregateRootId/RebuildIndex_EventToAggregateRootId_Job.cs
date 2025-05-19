using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.Cluster.Job;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.EventStore.Index;

public class RebuildIndex_EventToAggregateRootId_Job : CronusJob<RebuildIndex_JobData>
{
    private readonly IEventStorePlayer eventStorePlayer;
    private readonly EventLookupInByteArray eventFinder;
    private readonly IIndexStore indexStore;

    public RebuildIndex_EventToAggregateRootId_Job(IEventStorePlayer eventStorePlayer, EventLookupInByteArray eventFinder, IIndexStore indexStore, ILogger<RebuildIndex_EventToAggregateRootId_Job> logger) : base(logger)
    {
        this.eventStorePlayer = eventStorePlayer;
        this.eventFinder = eventFinder;
        this.indexStore = indexStore;
    }

    public override string Name { get; set; } = typeof(EventToAggregateRootId).GetContractId();

    protected override async Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("A job has been cancelled.");
            return JobExecutionStatus.Running;
        }

        long startTimestamp = 0L;
        uint counter = 0u;
        PlayerOperator @operator = new PlayerOperator()
        {
            OnLoadAsync = async @event =>
            {
                string eventContractId = eventFinder.FindEventId(@event.Data.AsSpan());
                if (string.IsNullOrEmpty(eventContractId))
                {
                    logger.LogError($"Unable to find a valid event in the data : {Encoding.UTF8.GetString(@event.Data)}");
                    return;
                }

                IndexRecord indexRecord = new IndexRecord(eventContractId, @event.AggregateRootId, @event.Revision, @event.Position, @event.Timestamp);
                await indexStore.ApendAsync(indexRecord);

                Interlocked.Increment(ref counter);
            },
            NotifyProgressAsync = async options =>
            {
                var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
                Data.PaginationToken = options.PaginationToken;
                Data.MaxDegreeOfParallelism = options.MaxDegreeOfParallelism;
                Data.Timestamp = DateTimeOffset.UtcNow;
                Data.ProcessedCount = counter;

                Data = await cluster.PingAsync(Data).ConfigureAwait(false);

                if (logger.IsEnabled(LogLevel.Information))
                {
                    var avgSpeed = Math.Round(counter / elapsed.TotalSeconds, 1); // no need to check for division by 0. double.PositiveInfinity is a thing
                    logger.LogInformation("RebuildIndex_EventToAggregateRootId_Job progress: {counter}. Average speed {speed} events/s.", counter, avgSpeed);
                }
            }
        };

        PlayerOptions options = new PlayerOptions
        {
            PaginationToken = Data.PaginationToken,
            MaxDegreeOfParallelism = Data.MaxDegreeOfParallelism
        };

        logger.LogInformation("Max degree of parallelism is {max_dop}.", options.MaxDegreeOfParallelism);

        startTimestamp = Stopwatch.GetTimestamp();
        await eventStorePlayer.EnumerateEventStore(@operator, options).ConfigureAwait(false);
        var elapsed = Stopwatch.GetElapsedTime(startTimestamp);

        Data.IsCompleted = true;
        Data = await cluster.PingAsync(Data).ConfigureAwait(false);

        var avgSpeed = Math.Round(counter / elapsed.TotalSeconds, 1); // no need to check for division by 0. double.PositiveInfinity is a thing

        logger.LogInformation("The job has been completed. Processed {counter}. Average speed {speed} events/s.", counter, avgSpeed);

        return JobExecutionStatus.Completed;
    }
}
