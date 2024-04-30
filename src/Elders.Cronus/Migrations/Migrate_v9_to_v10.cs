using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.Projections;
using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations;

public sealed class Migrate_v9_to_v10 : IMigrationCustomLogic, IMigration<AggregateCommit>
{
    private static DateTimeOffset MinFiletimeAsDateTimeOffset = new DateTimeOffset(1601, 2, 1, 0, 0, 0, 0, TimeSpan.Zero);

    const string TimestampPropertyName = "Timestamp";

    private readonly IEventStoreFactory eventStoreFactory;
    private readonly IIndexStore indexStore;
    private readonly IInitializableProjectionStore initializableProjectionStore;
    private readonly IProjectionWriter projection;
    private readonly TypeContainer<IProjection> projectionsContainer;
    private readonly LatestProjectionVersionFinder projectionFinder;
    private readonly ILogger<Migrate_v9_to_v10> logger;

    public Migrate_v9_to_v10(IEventStoreFactory eventStoreFactory, IIndexStore indexStore, IInitializableProjectionStore initializableProjectionStore, IProjectionWriter projection, TypeContainer<IProjection> projectionsContainer, LatestProjectionVersionFinder projectionFinder, ILogger<Migrate_v9_to_v10> logger)
    {
        this.eventStoreFactory = eventStoreFactory;
        this.indexStore = indexStore;
        this.initializableProjectionStore = initializableProjectionStore;
        this.projection = projection;
        this.projectionsContainer = projectionsContainer;
        this.projectionFinder = projectionFinder;
        this.logger = logger;
    }

    List<ProjectionVersion> liveOnlyProjections = null;

    public async Task OnAggregateCommitAsync(AggregateCommit migratedAggregateCommit)
    {
        List<Task> tasks = new List<Task>();

        // fix timestamp
        int timestampFix = 0;
        foreach (IEvent @event in migratedAggregateCommit.Events)
        {
            timestampFix++;
            if (@event.Timestamp <= MinFiletimeAsDateTimeOffset)
            {
                var propInfo = @event.GetType().GetProperty(TimestampPropertyName);
                if (propInfo is not null)
                {
                    propInfo.SetValue(@event, (migratedAggregateCommit.Timestamp + timestampFix).ToDateTimeOffsetUtc());
                }
            }
        }

        foreach (IPublicEvent publicEvent in migratedAggregateCommit.PublicEvents)
        {
            timestampFix++;
            if (publicEvent.Timestamp <= MinFiletimeAsDateTimeOffset)
            {
                var propInfo = publicEvent.GetType().GetProperty(TimestampPropertyName);
                if (propInfo is not null)
                {
                    propInfo.SetValue(publicEvent, (migratedAggregateCommit.Timestamp + timestampFix).ToDateTimeOffsetUtc());
                }
            }
        }

        // EventStore
        Task task = eventStoreFactory.GetEventStore().AppendAsync(migratedAggregateCommit);
        tasks.Add(task);

        // Index
        for (int pos = 0; pos < migratedAggregateCommit.Events.Count; pos++)
        {
            var record = new IndexRecord(migratedAggregateCommit.Events[pos].Unwrap().GetType().GetContractId(), migratedAggregateCommit.AggregateRootId, migratedAggregateCommit.Revision, pos, migratedAggregateCommit.Timestamp);
            tasks.Add(indexStore.ApendAsync(record));
        }

        for (int ppos = 0; ppos < migratedAggregateCommit.PublicEvents.Count; ppos++)
        {
            int publicEventPosition = (migratedAggregateCommit.Events.Count - 1) + 5 + ppos;
            var record = new IndexRecord(migratedAggregateCommit.PublicEvents[ppos].GetType().GetContractId(), migratedAggregateCommit.AggregateRootId, migratedAggregateCommit.Revision, publicEventPosition, migratedAggregateCommit.Timestamp);
            tasks.Add(indexStore.ApendAsync(record));
        }

        // projection
        if (liveOnlyProjections is null || liveOnlyProjections.Count == 0)
        {
            var allProjections = projectionFinder.GetProjectionVersionsToBootstrap();
            liveOnlyProjections = allProjections.Where(ver => ver.Status == ProjectionStatus.Live).ToList();

            var nonLiveProjections = allProjections.Except(liveOnlyProjections);
            if (nonLiveProjections.Any())
            {
                StringBuilder reporter = new StringBuilder();
                reporter.AppendLine("The following projection versions will not be migrated because their status is not live. Please handle them manually.");
                foreach (var projection in nonLiveProjections)
                {
                    reporter.AppendLine(projection.ToString());
                }
                logger.Warn(() => reporter.ToString());
            }

            foreach (ProjectionVersion liveVersion in liveOnlyProjections)
            {
                await initializableProjectionStore.InitializeAsync(liveVersion).ConfigureAwait(false);
            }
        }

        IEnumerable<Type> projectionTypes = projectionsContainer.Items;
        for (int pos = 0; pos < migratedAggregateCommit.Events.Count; pos++)
        {
            foreach (var projectionType in projectionTypes)
            {
                bool isInterested = projectionType.GetInterfaces()
                    .Where(@interface => IsInterested(@interface, migratedAggregateCommit.Events[pos].GetType()))
                    .Any();

                if (isInterested)
                {
                    ProjectionVersion version = liveOnlyProjections.Where(ver => ver.ProjectionName.Equals(projectionType.GetContractId())).SingleOrDefault();
                    if (version is not null)
                    {
                        Task projectionTask = projection.SaveAsync(projectionType, migratedAggregateCommit.Events[pos], version);
                        tasks.Add(projectionTask);
                    }
                }
            }
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public AggregateCommit Apply(AggregateCommit current) => current;

    public bool ShouldApply(AggregateCommit current) => true;

    private static bool IsInterested(Type handlerInterfaces, Type messagePayloadType)
    {
        var genericArguments = handlerInterfaces.GetGenericArguments();

        return handlerInterfaces.IsGenericType && genericArguments.Length == 1 && messagePayloadType.IsAssignableFrom(genericArguments[0]);
    }
}

[DataContract(Name = "ad7b4d5f-da19-4fb2-b4ae-f90addf66c05")]
public sealed class DoFixEventTimestamps : ISignal
{
    DoFixEventTimestamps()
    {
        Timestamp = DateTimeOffset.UtcNow;
    }

    public DoFixEventTimestamps(string tenant) : this()
    {
        Tenant = tenant;
    }

    [DataMember(Order = 0)]
    public string Tenant { get; private set; }

    [DataMember(Order = 1)]
    public DateTimeOffset Timestamp { get; private set; }
}

[DataContract(Name = "6c76a1cb-0fe8-4f34-8b60-fcb285c75d08")]
public class FixEventTimestamps : ITrigger,
        ISignalHandle<DoFixEventTimestamps>
{
    private readonly IEventStorePlayer player;
    private readonly IEventStore eventStore;
    private readonly ISerializer serializer;
    private readonly ILogger<FixEventTimestamps> logger;

    public FixEventTimestamps(IEventStorePlayer player, ILogger<FixEventTimestamps> logger, IEventStore eventStore, ISerializer serializer)
    {
        this.player = player;
        this.logger = logger;
        this.eventStore = eventStore;
        this.serializer = serializer;
    }
    const string TimestampPropertyName = "Timestamp";

    public async Task HandleAsync(DoFixEventTimestamps signal)
    {
        logger.Info(() => "Starting FixEventTimestamps...");

        PlayerOperator @operator = new PlayerOperator()
        {
            OnAggregateStreamLoadedAsync = async arStream =>
            {
                foreach (AggregateCommitRaw commit in arStream.Commits)
                {
                    byte[] arid = null;
                    int rev = -1;

                    Dictionary<int, IMessage> messages = new Dictionary<int, IMessage>();
                    HashSet<DateTimeOffset> timestamps = new HashSet<DateTimeOffset>();
                    foreach (var @event in commit.Events)
                    {
                        if (rev < 0)
                        {
                            arid = @event.AggregateRootId;
                            rev = @event.Revision;
                        }

                        var temp = serializer.DeserializeFromBytes<IMessage>(@event.Data);
                        messages.Add(@event.Position, temp);

                        timestamps.Add(temp.Timestamp);
                    }

                    if (messages.Count != timestamps.Count)
                    {
                        // fix timestamp
                        int timestampFix = 0;
                        foreach (var msg in messages)
                        {
                            timestampFix++;

                            var propInfo = msg.Value.GetType().GetProperty(TimestampPropertyName);
                            if (propInfo is not null)
                            {
                                propInfo.SetValue(msg.Value, msg.Value.Timestamp.AddTicks(timestampFix));
                            }

                            var newEventFixedTimestamp = new AggregateEventRaw(arid, serializer.SerializeToBytes(msg.Value), rev, msg.Key, commit.Timestamp.ToFileTime());
                            // EventStore
                            await eventStore.AppendAsync(newEventFixedTimestamp).ConfigureAwait(false);
                        }
                    }
                }

                //if (count % 100 == 0)
                //{
                //    logger.LogInformation("Deleted and reordered events for {count} offer ARs", count);
                //}
            }
        };

        await player.EnumerateEventStore(@operator, new PlayerOptions());

        logger.Info(() => "Finished FixEventTimestamps....");
    }
}
