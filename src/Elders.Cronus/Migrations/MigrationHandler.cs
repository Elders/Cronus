using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.Projections;
using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.Migrations
{
    [DataContract(Name = "2f26cd18-0db8-425f-8ada-5e3bf06a57b5")]
    public sealed class MigrationHandler : IMigrationHandler,
        IAggregateCommitHandle<AggregateCommit>
    {
        private readonly CronusMigrator cronusMigrator;

        public MigrationHandler(CronusMigrator cronusMigrator)
        {
            this.cronusMigrator = cronusMigrator;
        }

        public Task HandleAsync(AggregateCommit aggregateCommit)
        {
            return cronusMigrator.MigrateAsync(aggregateCommit);
        }
    }

    public interface IMigrationCustomLogic
    {
        Task OnAggregateCommitAsync(AggregateCommit migratedAggregateCommit);
    }

    public sealed class CronusMigrator
    {
        private readonly IEnumerable<IMigration<AggregateCommit>> migrations;
        private readonly IMigrationCustomLogic theLogic;
        private readonly ILogger<MigrationHandler> logger;

        public CronusMigrator(IEnumerable<IMigration<AggregateCommit>> migrations, IMigrationCustomLogic theLogic, ILogger<MigrationHandler> logger)
        {
            this.migrations = migrations;
            this.theLogic = theLogic;
            this.logger = logger;
        }

        public Task MigrateAsync(AggregateCommit aggregateCommit)
        {
            foreach (var migration in migrations)
            {
                if (migration.ShouldApply(aggregateCommit))
                    aggregateCommit = migration.Apply(aggregateCommit);
            }

            try
            {
                return theLogic.OnAggregateCommitAsync(aggregateCommit);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => "We do not trust people that inject their custom logic into important LOOOOONG running processes like this one.");
                return Task.FromException(ex);
            }
        }
    }

    public sealed class MigrateAggregateCommitFrom_Cronus_v8_to_v9 : IMigrationCustomLogic, IMigration<AggregateCommit>
    {
        private readonly EventStoreFactory eventStoreFactory;
        private readonly IIndexStore indexStore;
        private readonly IProjectionWriter projection;
        private readonly TypeContainer<IProjection> projectionsContainer;
        private readonly LatestProjectionVersionFinder projectionFinder;

        public MigrateAggregateCommitFrom_Cronus_v8_to_v9(EventStoreFactory eventStoreFactory, IIndexStore indexStore, IProjectionWriter projection, TypeContainer<IProjection> projectionsContainer, LatestProjectionVersionFinder projectionFinder)
        {
            this.eventStoreFactory = eventStoreFactory;
            this.indexStore = indexStore;
            this.projection = projection;
            this.projectionsContainer = projectionsContainer;
            this.projectionFinder = projectionFinder;
        }

        List<ProjectionVersion> allProjectionVersions = null;

        public async Task OnAggregateCommitAsync(AggregateCommit migratedAggregateCommit)
        {
            List<Task> tasks = new List<Task>();

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
                var record = new IndexRecord(migratedAggregateCommit.Events[ppos].Unwrap().GetType().GetContractId(), migratedAggregateCommit.AggregateRootId, migratedAggregateCommit.Revision, publicEventPosition, migratedAggregateCommit.Timestamp);
                tasks.Add(indexStore.ApendAsync(record));
            }

            // projection
            if (allProjectionVersions is null || allProjectionVersions.Count == 0)
            {
                allProjectionVersions = projectionFinder.GetProjectionVersionsToBootstrap().ToList();
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
                        var origin = new EventOrigin(migratedAggregateCommit.AggregateRootId, migratedAggregateCommit.Revision, pos, migratedAggregateCommit.Timestamp);
                        var version = allProjectionVersions.Where(ver => ver.ProjectionName.Equals(projectionType.GetContractId())).Single();
                        var projectionTask = projection.SaveAsync(projectionType, migratedAggregateCommit.Events[pos], origin, version);
                        tasks.Add(projectionTask);
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
}
