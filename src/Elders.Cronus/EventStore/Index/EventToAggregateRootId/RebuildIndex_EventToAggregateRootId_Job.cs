using Elders.Cronus.Cluster.Job;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    public class EventFinder
    {
        ConcurrentDictionary<string, byte[]> messageToByteArray = new ConcurrentDictionary<string, byte[]>();

        public EventFinder(TypeContainer<IEvent> events, TypeContainer<IPublicEvent> publicEvents)
        {
            Type entityEvent = typeof(EntityEvent);
            foreach (Type eventType in events.Items)
            {
                if (eventType == entityEvent)
                    continue;

                string contractId = eventType.GetContractId();
                byte[] bytes = Encoding.UTF8.GetBytes(contractId);
                messageToByteArray.TryAdd(contractId, bytes);
            }
            foreach (Type eventType in publicEvents.Items)
            {
                string contractId = eventType.GetContractId();
                byte[] bytes = Encoding.UTF8.GetBytes(contractId);
                messageToByteArray.TryAdd(contractId, bytes);
            }
        }

        public string FindEvent(byte[] source)
        {
            foreach (var item in messageToByteArray)
            {
                if (FindSequence(source, 0, item.Value) > -1)
                    return item.Key;
            }

            return string.Empty;
        }

        /// <summary>Looks for the next occurrence of a sequence in a byte array.</summary>
        /// <param name="array">Array that will be scanned</param>
        /// <param name="start">Index in the array at which scanning will begin</param>
        /// <param name="sequence">Sequence the array will be scanned for</param>
        /// <returns>
        /// The index of the next occurrence of the sequence of -1 if not found
        /// </returns>
        private static int FindSequence(byte[] array, int start, byte[] sequence)
        {
            int end = array.Length - sequence.Length; // past here no match is possible
            byte firstByte = sequence[0]; // cached to tell compiler there's no aliasing

            while (start <= end)
            {
                // scan for first byte only. compiler-friendly.
                if (array[start] == firstByte)
                {
                    // scan for rest of sequence
                    for (int offset = 1; ; ++offset)
                    {
                        if (offset == sequence.Length)
                        {
                            return start; // full sequence matched?
                        }
                        else if (array[start + offset] != sequence[offset])
                        {
                            break;
                        }
                    }
                }
                ++start;
            }

            return -1; // end of array reached without match
        }

    }

    public class RebuildIndex_EventToAggregateRootId_Job : CronusJob<RebuildIndex_JobData>
    {
        private readonly IEventStorePlayer eventStorePlayer;
        private readonly EventFinder eventFinder;
        private readonly IIndexStore indexStore;

        public RebuildIndex_EventToAggregateRootId_Job(IEventStorePlayer eventStorePlayer, EventFinder eventFinder, IIndexStore indexStore, ILogger<RebuildIndex_EventToAggregateRootId_Job> logger) : base(logger)
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
                    if (counter % 1000 == 0)
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
