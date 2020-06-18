using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.EventStore.Index.Handlers;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections
{
    public class ProjectionPlayer : IProjectionPlayer
    {
        static readonly ILogger logger = CronusLogger.CreateLogger(typeof(ProjectionPlayer));

        private readonly CronusContext context;
        private readonly IEventStore eventStore;
        private readonly IProjectionWriter projectionWriter;
        private readonly IProjectionReader projectionReader;
        private readonly IInitializableProjectionStore projectionStoreInitializer;
        private readonly EventToAggregateRootId index;

        public ProjectionPlayer(CronusContext context, IEventStore eventStore, IProjectionWriter projectionRepository, IProjectionReader projectionReader, IInitializableProjectionStore projectionStoreInitializer, EventToAggregateRootId index)
        {
            this.context = context;
            this.eventStore = eventStore;
            this.projectionWriter = projectionRepository;
            this.projectionReader = projectionReader;
            this.projectionStoreInitializer = projectionStoreInitializer;
            this.index = index;
        }

        public ReplayResult Rebuild(ProjectionVersion version, DateTime rebuildUntil)
        {
            if (ReferenceEquals(null, version)) throw new ArgumentNullException(nameof(version));

            Type projectionType = version.ProjectionName.GetTypeByContract();
            try
            {
                IndexStatus indexStatus = GetIndexStatus<EventToAggregateRootId>();
                if (indexStatus.IsNotPresent() && IsNotSystemProjection(projectionType)) return ReplayResult.RetryLater($"The index is not present");

                if (IsVersionTrackerMissing() && IsNotSystemProjection(projectionType)) return ReplayResult.RetryLater($"Projection `{version}` still don't have present index."); //WHEN TO RETRY AGAIN
                if (HasReplayTimeout(rebuildUntil)) return ReplayResult.Timeout($"Rebuild of projection `{version}` has expired. Version:{version} Deadline:{rebuildUntil}.");

                var allVersions = GetAllVersions(version);
                if (allVersions.IsOutdatad(version)) return new ReplayResult($"Version `{version}` is outdated. There is a newer one which is already live.");
                if (allVersions.IsCanceled(version)) return new ReplayResult($"Version `{version}` was canceled.");

                DateTime startRebuildTimestamp = DateTime.UtcNow;
                int progressCounter = 0;
                logger.Info(() => $"Start rebuilding projection `{version.ProjectionName}` for version {version}. Deadline is {rebuildUntil}");
                Dictionary<int, string> processedAggregates = new Dictionary<int, string>();

                projectionStoreInitializer.Initialize(version);

                var projectionHandledEventTypes = GetInvolvedEvents(projectionType);
                foreach (var eventType in projectionHandledEventTypes)
                {
                    logger.Info(() => $"Rebuilding projection `{version.ProjectionName}` for version {version} using eventType `{eventType}`. Deadline is {rebuildUntil}");

                    IEnumerable<IndexRecord> indexRecords = index.EnumerateRecords(eventType);
                    foreach (IndexRecord indexRecord in indexRecords)
                    {
                        // TODO: (5) Decorator pattern which will give us the tracking 
                        progressCounter++;
                        if (progressCounter % 1000 == 0)
                        {
                            logger.Info(() => $"Rebuilding projection {version.ProjectionName} => PROGRESS:{progressCounter} Version:{version} EventType:{eventType} Deadline:{rebuildUntil} Total minutes working:{(DateTime.UtcNow - startRebuildTimestamp).TotalMinutes}. logId:{Guid.NewGuid().ToString()} ProcessedAggregatesSize:{processedAggregates.Count}");
                        }

                        int aggreagteRootIdHash = indexRecord.AggregateRootId.GetHashCode();
                        if (processedAggregates.ContainsKey(aggreagteRootIdHash))
                            continue;
                        processedAggregates.Add(aggreagteRootIdHash, null);

                        string mess = Encoding.UTF8.GetString(indexRecord.AggregateRootId);
                        IAggregateRootId arId = GetAggregateRootId(mess);
                        EventStream stream = eventStore.Load(arId);

                        foreach (AggregateCommit arCommit in stream.Commits)
                        {
                            for (int i = 0; i < arCommit.Events.Count; i++)
                            {
                                IEvent theEvent = arCommit.Events[i].Unwrap();

                                if (projectionHandledEventTypes.Contains(theEvent.GetType().GetContractId())) // filters out the events which are not part of the projection
                                {
                                    var origin = new EventOrigin(mess, arCommit.Revision, i, arCommit.Timestamp);
                                    projectionWriter.Save(projectionType, theEvent, origin, version);
                                }
                            }
                        }
                    }
                }

                logger.Info(() => $"Finish rebuilding projection `{projectionType.Name}` for version {version}. Deadline was {rebuildUntil}");
                return new ReplayResult();
            }
            catch (Exception ex)
            {
                string message = $"Unable to replay projection. Version:{version} ProjectionType:{projectionType.FullName}";
                logger.ErrorException(ex, () => message);
                return new ReplayResult(message + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private bool HasReplayTimeout(DateTime replayUntil)
        {
            return DateTime.UtcNow >= replayUntil;
        }

        IEnumerable<string> GetInvolvedEvents(Type projectionType)
        {
            var ieventHandler = typeof(IEventHandler<>);
            var interfaces = projectionType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler);
            foreach (var @interface in interfaces)
            {
                Type eventType = @interface.GetGenericArguments().First();
                yield return eventType.GetContractId();
            }
        }

        IAggregateRootId GetAggregateRootId(string mess)
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

        ProjectionVersions GetAllVersions(ProjectionVersion version)
        {
            var versionId = new ProjectionVersionManagerId(version.ProjectionName, context.Tenant);
            var result = projectionReader.Get<ProjectionVersionsHandler>(versionId);
            if (result.IsSuccess)
                return result.Data.State.AllVersions;

            return new ProjectionVersions();
        }

        IndexStatus GetIndexStatus<TIndex>() where TIndex : IEventStoreIndex
        {
            var id = new EventStoreIndexManagerId(typeof(TIndex).GetContractId(), context.Tenant);
            var result = projectionReader.Get<EventStoreIndexStatus>(id);
            if (result.IsSuccess)
                return result.Data.State.Status;

            return IndexStatus.NotPresent;
        }

        bool IsNotSystemProjection(Type projectionType)
        {
            return typeof(ISystemProjection).IsAssignableFrom(projectionType) == false;
        }

        bool IsVersionTrackerMissing()
        {
            var versionId = new ProjectionVersionManagerId(ProjectionVersionsHandler.ContractId, context.Tenant);
            var result = projectionReader.Get<ProjectionVersionsHandler>(versionId);

            return result.HasError || result.NotFound;
        }
    }
}
