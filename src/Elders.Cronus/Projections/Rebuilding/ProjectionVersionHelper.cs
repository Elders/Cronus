using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Elders.Cronus.EventStore.Index.Handlers;
using Elders.Cronus.Projections.Versioning;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.EventStore.Index;

namespace Elders.Cronus.Projections.Rebuilding
{
    public class ProjectionVersionHelper
    {
        private readonly CronusContext context;
        private readonly ProjectionHasher projectionHasher;
        private readonly IInitializableProjectionStore projectionVersionInitializer;
        private readonly IProjectionReader projectionReader;
        private readonly ILogger<ProjectionVersionHelper> logger;

        public ProjectionVersionHelper(CronusContext context, IProjectionReader projectionReader, IInitializableProjectionStore projectionVersionInitializer, ProjectionHasher projectionHasher, ILogger<ProjectionVersionHelper> logger)
        {
            this.context = context;
            this.projectionReader = projectionReader;
            this.projectionVersionInitializer = projectionVersionInitializer;
            this.projectionHasher = projectionHasher;
            this.logger = logger;
        }

        /// <summary>
        /// Initializing new projection version if needed
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public void InitializeNewProjectionVersion()
        {
            ProjectionVersion newPersistentVersion = GetNewProjectionVersion();
            projectionVersionInitializer.Initialize(newPersistentVersion);
        }

        public bool ShouldBeRetried(ProjectionVersion version)
        {
            if (IsVersionTrackerMissing())
            {
                if (version.ProjectionName.Equals(ProjectionVersionsHandler.ContractId, StringComparison.OrdinalIgnoreCase) == false)
                    return true;

                InitializeNewProjectionVersion();
            }

            IndexStatus indexStatus = GetIndexStatus<EventToAggregateRootId>();
            Type projectionType = version.ProjectionName.GetTypeByContract();

            if (IsVersionTrackerMissing() && IsNotSystemProjection(projectionType)) return true;
            if (indexStatus.IsNotPresent() && IsNotSystemProjection(projectionType)) return true;

            return false;
        }

        public bool ShouldBeCanceled(ProjectionVersion version, DateTimeOffset dueDate)
        {
            if (HasReplayTimeout(dueDate))
            {
                logger.Error(() => $"Rebuild of projection `{version}` has expired. Version:{version} Deadline:{dueDate}.");
                return true;
            }

            var allVersions = GetAllVersions(version);
            if (allVersions.IsOutdatad(version))
            {
                logger.Error(() => $"Version `{version}` is outdated. There is a newer one which is already live.");
                return true;
            }
            if (allVersions.IsCanceled(version) && version.ProjectionName.Equals(ProjectionVersionsHandler.ContractId, StringComparison.OrdinalIgnoreCase) == false)
            {
                logger.Error(() => $"Version `{version}` was canceled.false");
                return true;
            }

            return false;
        }

        public IEnumerable<Type> GetInvolvedEventTypes(Type projectionType)
        {
            var ieventHandler = typeof(IEventHandler<>);
            var interfaces = projectionType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler);
            foreach (var @interface in interfaces)
            {
                Type eventType = @interface.GetGenericArguments().First();
                yield return eventType;
            }
        }

        private ProjectionVersion GetNewProjectionVersion()
        {
            return new ProjectionVersion(ProjectionVersionsHandler.ContractId, ProjectionStatus.Live, 1, projectionHasher.CalculateHash(typeof(ProjectionVersionsHandler)));
        }

        private bool IsVersionTrackerMissing()
        {
            var versionId = new ProjectionVersionManagerId(ProjectionVersionsHandler.ContractId, context.Tenant);
            var result = projectionReader.Get<ProjectionVersionsHandler>(versionId);

            return result.HasError || result.NotFound;
        }

        private bool HasReplayTimeout(DateTimeOffset replayUntil)
        {
            return DateTimeOffset.UtcNow >= replayUntil;
        }

        private bool IsNotSystemProjection(Type projectionType)
        {
            return typeof(ISystemProjection).IsAssignableFrom(projectionType) == false;
        }

        private IndexStatus GetIndexStatus<TIndex>() where TIndex : IEventStoreIndex
        {
            var id = new EventStoreIndexManagerId(typeof(TIndex).GetContractId(), context.Tenant);
            var result = projectionReader.Get<EventStoreIndexStatus>(id);
            if (result.IsSuccess)
                return result.Data.State.Status;

            return IndexStatus.NotPresent;
        }

        private ProjectionVersions GetAllVersions(ProjectionVersion version)
        {
            var versionId = new ProjectionVersionManagerId(version.ProjectionName, context.Tenant);
            var result = projectionReader.Get<ProjectionVersionsHandler>(versionId);
            if (result.IsSuccess)
                return result.Data.State.AllVersions;

            return new ProjectionVersions();
        }
    }
}
