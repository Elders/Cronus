using System;
using System.Collections.Generic;
using System.Linq;
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

    private readonly EventStoreFactory eventStoreFactory;
    private readonly IIndexStore indexStore;
    private readonly IInitializableProjectionStore initializableProjectionStore;
    private readonly IProjectionWriter projection;
    private readonly TypeContainer<IProjection> projectionsContainer;
    private readonly LatestProjectionVersionFinder projectionFinder;
    private readonly ILogger<Migrate_v9_to_v10> logger;

    public Migrate_v9_to_v10(EventStoreFactory eventStoreFactory, IIndexStore indexStore, IInitializableProjectionStore initializableProjectionStore, IProjectionWriter projection, TypeContainer<IProjection> projectionsContainer, LatestProjectionVersionFinder projectionFinder, ILogger<Migrate_v9_to_v10> logger)
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
        foreach (IEvent @event in migratedAggregateCommit.Events)
        {
            if (@event.Timestamp <= MinFiletimeAsDateTimeOffset)
            {
                var propInfo = @event.GetType().GetProperty(TimestampPropertyName);
                if (propInfo is not null)
                {
                    propInfo.SetValue(@event, migratedAggregateCommit.Timestamp.ToDateTimeOffsetUtc());
                }
            }
        }

        foreach (IPublicEvent publicEvent in migratedAggregateCommit.PublicEvents)
        {
            if (publicEvent.Timestamp <= MinFiletimeAsDateTimeOffset)
            {
                var propInfo = publicEvent.GetType().GetProperty(TimestampPropertyName);
                if (propInfo is not null)
                {
                    propInfo.SetValue(publicEvent, migratedAggregateCommit.Timestamp.ToDateTimeOffsetUtc());
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
                    EventOrigin origin = new EventOrigin(migratedAggregateCommit.AggregateRootId, migratedAggregateCommit.Revision, pos, migratedAggregateCommit.Events[pos].Timestamp.ToFileTime());
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
