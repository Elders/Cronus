using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.EventStore;

namespace Elders.Cronus.Projections.Rebuilding
{
    public class RebuildingProjectionRepository : IRebuildingProjectionRepository
    {
        private readonly ProgressTracker progressTracker;
        private readonly ProjectionIndex index;
        private readonly IEventStore eventStore;
        private readonly ILogger logger;
        private readonly Dictionary<int, string> processedAggregates;

        public RebuildingProjectionRepository(ProjectionIndex index, IEventStore eventStore, ProgressTracker progressTracker, ILogger<RebuildingProjectionRepository> logger)
        {
            this.progressTracker = progressTracker;
            this.eventStore = eventStore;
            this.index = index;
            this.logger = logger;

            processedAggregates = new Dictionary<int, string>();
        }

        public async Task<IEnumerable<EventStream>> LoadEventsAsync(IEnumerable<IndexRecord> indexRecords, ProjectionVersion version)
        {
            List<Task<EventStream>> streamTasks = new List<Task<EventStream>>();

            foreach (IndexRecord indexRecord in indexRecords)
            {
                try
                {
                    if (ShouldLoadAggregate(indexRecord.AggregateRootId.GetHashCode()) == false)
                        continue;

                    string mess = Encoding.UTF8.GetString(indexRecord.AggregateRootId);
                    IAggregateRootId arId = GetAggregateRootId(mess);
                    streamTasks.Add(eventStore.LoadAsync(arId));
                }
                catch (Exception ex) when (logger.WarnException(ex, () => $"{indexRecord.AggregateRootId} was skipped when rebuilding {version.ProjectionName}.")) { }
            }

            await Task.WhenAll(streamTasks.ToArray()).ConfigureAwait(false);
            return streamTasks.Select(t => t.Result);
        }

        private bool ShouldLoadAggregate(int aggreagteRootIdHash)
        {
            if (processedAggregates.ContainsKey(aggreagteRootIdHash) == false)
            {
                processedAggregates.Add(aggreagteRootIdHash, null);
                return true;
            }

            return false;
        }

        private IAggregateRootId GetAggregateRootId(string mess)
        {
            var parts = mess.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                AggregateUrn urn;
                if (AggregateUrn.TryParse(part, out urn))
                {
                    return new AggregateRootId(urn.AggregateRootName, urn);
                }
                else
                {
                    byte[] raw = Convert.FromBase64String(part);
                    string urnString = Encoding.UTF8.GetString(raw);
                    if (AggregateUrn.TryParse(urnString, out urn))
                    {
                        return new AggregateRootId(urn.AggregateRootName, urn);
                    }
                }
            }

            throw new ArgumentException($"Invalid aggregate root id: {mess}", nameof(mess));
        }

        const ushort _batchSize = 1000;
        public async Task SaveAggregateCommitsAsync(IEnumerable<IndexRecord> eventStreams, RebuildProjection_JobData jobData)
        {
            List<Func<Task<string>>> indexingTasks = new List<Func<Task<string>>>(_batchSize);

            try
            {
                ushort currentSize = 0;
                foreach (IndexRecord record in eventStreams)
                {
                    currentSize++;

                    if (currentSize % _batchSize == 0)
                    {
                        await progressTracker.CompleteActionWithProgressSignalAsync(indexingTasks).ConfigureAwait(false);
                        indexingTasks.Clear();

                        if (jobData.IsCanceled)
                            return;
                    }
                    else
                    {
                        indexingTasks.Add(() => index.IndexAsync(record, jobData.Version));
                    }
                }
            }
            catch (Exception ex) when (logger.WarnException(ex, () => $"Index record was skipped when rebuilding {jobData.Version.ProjectionName}.")) { }
        }
    }
}
