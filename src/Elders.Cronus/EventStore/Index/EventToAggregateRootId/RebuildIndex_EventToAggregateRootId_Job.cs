using Elders.Cronus.Cluster.Job;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
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
                logger.Info(() => $"The job has been cancelled.");
                return JobExecutionStatus.Running;
            }

            uint counter = 0;
            PlayerOperator @operator = new PlayerOperator()
            {
                OnLoadAsync = async @event =>
                {
                    string eventContractId = eventFinder.FindEvent(@event.Data);
                    if (string.IsNullOrEmpty(eventContractId))
                        logger.Error(() => $"Unable to find a valid event in the data : {Encoding.UTF8.GetString(@event.Data)}");

                    IndexRecord indexRecord = new IndexRecord(eventContractId, @event.AggregateRootId, @event.Revision, @event.Position, @event.Timestamp);
                    await indexStore.ApendAsync(indexRecord);

                    counter++;
                },
                NotifyProgressAsync = async options =>
                {
                    Data.PaginationToken = options.PaginationToken;
                    Data = await cluster.PingAsync(Data).ConfigureAwait(false);

                    logger.Info(() => $"RebuildIndex_EventToAggregateRootId_Job progress: {counter}");
                }
            };

            PlayerOptions options = new PlayerOptions().WithPaginationToken(Data.PaginationToken);
            await eventStorePlayer.EnumerateEventStore(@operator, options).ConfigureAwait(false);

            Data.IsCompleted = true;
            Data = await cluster.PingAsync(Data).ConfigureAwait(false);

            logger.Info(() => $"The job has been completed.");

            return JobExecutionStatus.Completed;
        }
    }
}
