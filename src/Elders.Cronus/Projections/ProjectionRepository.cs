using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Cassandra;
using Elders.Cronus.Projections.Versioning;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections
{
    public partial class ProjectionRepository : IProjectionWriter, IProjectionReader
    {
        private static readonly ILogger log = CronusLogger.CreateLogger(typeof(ProjectionRepository));
        private static readonly Action<ILogger, Exception> LogProjectionInstanceNotFound = LoggerMessage.Define(LogLevel.Debug, CronusLogEvent.CronusProjectionRead, "Projection instance not found.");
        private static readonly Action<ILogger, Exception> LogProjectionLoadError = LoggerMessage.Define(LogLevel.Error, CronusLogEvent.CronusProjectionRead, "Unable to load projection.");
        private static readonly Action<ILogger, Exception> LogProjectionLiveVersionMissing = LoggerMessage.Define(LogLevel.Error, CronusLogEvent.CronusProjectionRead, "Unable to find projection `live` version.");
        private static readonly Action<ILogger, Exception> LogProjectionWriteError = LoggerMessage.Define(LogLevel.Error, CronusLogEvent.CronusProjectionWrite, "Failed to write in projection.");

        private readonly ICronusContextAccessor contextAccessor;
        readonly IProjectionStore projectionStore;
        private readonly IHandlerFactory handlerFactory;
        private readonly ProjectionHasher projectionHasher;

        public ProjectionRepository(ICronusContextAccessor contextAccessor, IProjectionStore projectionStore, IHandlerFactory handlerFactory, ProjectionHasher projectionHasher)
        {
            if (contextAccessor is null) throw new ArgumentException(nameof(contextAccessor));
            if (projectionStore is null) throw new ArgumentException(nameof(projectionStore));

            this.contextAccessor = contextAccessor;
            this.projectionStore = projectionStore;
            this.handlerFactory = handlerFactory;
            this.projectionHasher = projectionHasher;
        }

        public async Task SaveAsync(Type projectionType, IEvent @event)
        {
            if (projectionType is null) throw new ArgumentNullException(nameof(projectionType));
            if (@event is null) throw new ArgumentNullException(nameof(@event));

            string projectionName = projectionType.GetContractId();
            // TODO: inspect the type if it implements IProjectionDefinition
            IHandlerInstance handlerInstance = handlerFactory.Create(projectionType);
            IProjectionDefinition projection = handlerInstance.Current as IProjectionDefinition;

            if (projection is not null)
            {
                var projectionIds = projection.GetProjectionIds(@event);

                List<Task> tasks = new List<Task>();
                foreach (IBlobId projectionId in projectionIds)
                {
                    using (log.BeginScope(s => s.AddScope(Log.ProjectionInstanceId, projectionId)))
                    {
                        ReadResult<ProjectionVersions> result = await GetProjectionVersionsAsync(projectionName).ConfigureAwait(false);
                        if (result.IsSuccess)
                        {
                            foreach (ProjectionVersion version in result.Data)
                            {
                                using (log.BeginScope(s => s.AddScope(Log.ProjectionVersion, version)))
                                {
                                    if (ShouldSaveEventForVersion(version))
                                    {
                                        Task task = PersistAsync(projectionId, version, @event);
                                        tasks.Add(task);
                                    }
                                }
                            }
                        }
                        if (result.HasError)
                        {
                            log.Error(() => "Failed to update projection because the projection version failed to load. Please replay the projection to restore the state. Self-heal hint!" + Environment.NewLine + result.Error + Environment.NewLine + $"\tProjectionName:{projectionName}" + Environment.NewLine + $"\tEvent:{@event}");
                        }
                    }
                }

                try
                {
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    StringBuilder taskErrors = new StringBuilder();
                    foreach (Task task in tasks)
                    {
                        if (task.IsFaulted)
                        {
                            taskErrors.AppendLine(task.Exception.ToString());
                        }
                    }

                    log.LogWarning(ex, "Failed to save event {event} in projection {projection}.", @event.GetType().Name, projectionType.Name);

                    throw;
                }
            }
        }

        /// <Remarks>
        /// Used by replay projections only.
        /// </Remarks>
        public async Task SaveAsync(Type projectionType, IEvent @event, ProjectionVersion version)
        {
            if (projectionType is null) throw new ArgumentNullException(nameof(projectionType));
            if (@event is null) throw new ArgumentNullException(nameof(@event));
            if (version is null) throw new ArgumentNullException(nameof(version));

            if (ShouldSaveEventForVersion(version) == false)
                throw new ArgumentException("Invalid version. Only versions in `Building` and `Live` status are eligable for persistence.", nameof(version));

            string projectionName = projectionType.GetContractId();
            if (projectionName.Equals(version.ProjectionName, StringComparison.OrdinalIgnoreCase) == false)
                throw new ArgumentException($"Invalid version. The version `{version}` does not match projection `{projectionName}`", nameof(version));

            bool isProjectionDefinitionType = typeof(IProjectionDefinition).IsAssignableFrom(projectionType);
            bool isEventSourcedType = typeof(IAmEventSourcedProjection).IsAssignableFrom(projectionType);

            if (isProjectionDefinitionType)
            {
                var handlerInstance = handlerFactory.Create(projectionType);
                var projection = handlerInstance.Current as IProjectionDefinition;

                var projectionIds = projection.GetProjectionIds(@event);
                List<Task> tasks = new List<Task>();
                foreach (var projectionId in projectionIds)
                {
                    Task task = PersistAsync(projectionId, version, @event);
                    tasks.Add(task);
                }

                try
                {
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    StringBuilder taskErrors = new StringBuilder();
                    foreach (Task task in tasks)
                    {
                        if (task.IsFaulted)
                        {
                            taskErrors.AppendLine(task.Exception.ToString());
                        }
                    }

                    log.LogWarning(ex, "Failed to save event {event} in projection {projection}.", @event.GetType().Name, projectionType.Name);

                    throw;
                }
            }
            else if (isEventSourcedType)
            {
                try
                {
                    var projectionId = new Urn($"urn:cronus:{projectionName}");

                    var commit = new ProjectionCommit(projectionId, version, @event);
                    await projectionStore.SaveAsync(commit).ConfigureAwait(false);
                }
                catch (Exception ex) when (ExceptionFilter.True(() => LogProjectionWriteError(log, ex))) { }
            }
        }

        public Task<ReadResult<T>> GetAsync<T>(IBlobId projectionId) where T : IProjectionDefinition
        {
            return GetInternalAsync<T>(projectionId, typeof(T));
        }

        public Task<ReadResult<IProjectionDefinition>> GetAsync(IBlobId projectionId, Type projectionType)
        {
            return GetInternalAsync<IProjectionDefinition>(projectionId, projectionType);
        }

        public Task<ReadResult<T>> GetAsOfAsync<T>(IBlobId projectionId, DateTimeOffset timestamp) where T : IProjectionDefinition
        {
            return GetAsOfInternalAsync<T>(projectionId, typeof(T), timestamp);
        }

        protected async virtual Task<ReadResult<ProjectionVersions>> GetProjectionVersionsAsync(string projectionName)
        {
            if (string.IsNullOrEmpty(projectionName)) throw new ArgumentNullException(nameof(projectionName));

            var queryResult = await GetProjectionVersionsFromStoreAsync(projectionName).ConfigureAwait(false);
            if (queryResult.IsSuccess)
                return new ReadResult<ProjectionVersions>(queryResult.Data.State.AllVersions);

            return ReadResult<ProjectionVersions>.WithError(queryResult.Error);
        }

        private async Task<ReadResult<T>> GetInternalAsync<T>(IBlobId projectionId, Type projectionType) where T : IProjectionDefinition
        {
            using (log.BeginScope(s => s
                       .AddScope(Log.ProjectionName, projectionType.GetContractId())
                       .AddScope(Log.ProjectionType, projectionType.Name)
                       .AddScope(Log.ProjectionInstanceId, projectionId.RawId)))
            {
                if (projectionId is null) throw new ArgumentNullException(nameof(projectionId));

                try
                {
                    ProjectionStream stream = await LoadProjectionStreamAsync(projectionId, projectionType).ConfigureAwait(false);
                    T projectionInstance = (T)FastActivator.CreateInstance(projectionType);
                    var readResult = new ReadResult<T>(await stream.RestoreFromHistoryAsync(projectionInstance).ConfigureAwait(false));
                    if (readResult.NotFound)
                        LogProjectionInstanceNotFound(log, null);

                    return readResult;
                }
                catch (Exception ex) when (ExceptionFilter.True(() => LogProjectionLoadError(log, ex)))
                {
                    return ReadResult<T>.WithError(ex);
                }
            }
        }

        private async Task<ReadResult<T>> GetAsOfInternalAsync<T>(IBlobId projectionId, Type projectionType, DateTimeOffset timestamp) where T : IProjectionDefinition
        {
            using (log.BeginScope(s => s
                       .AddScope(Log.ProjectionName, projectionType.GetContractId())
                       .AddScope(Log.ProjectionType, projectionType.Name)
                       .AddScope(Log.ProjectionInstanceId, projectionId.RawId)))
            {
                if (projectionId is null) throw new ArgumentNullException(nameof(projectionId));

                ProjectionStream stream = ProjectionStream.Empty();
                try
                {
                    ProjectionVersion liveVersion = await LoadLiveProjectionVersion(projectionType).ConfigureAwait(false);
                    if (liveVersion is not null)
                    {
                        ProjectionQueryOptions options = new ProjectionQueryOptions(projectionId, liveVersion, timestamp);
                        ProjectionsOperator @operator = new ProjectionsOperator()
                        {
                            OnProjectionStreamLoadedAsync = projectionStream =>
                            {
                                stream = projectionStream;

                                return Task.CompletedTask;
                            }
                        };
                        await projectionStore.EnumerateProjectionsAsync(@operator, options).ConfigureAwait(false);
                    }

                    T projectionInstance = (T)FastActivator.CreateInstance(projectionType);
                    var readResult = new ReadResult<T>(await stream.RestoreFromHistoryAsync(projectionInstance).ConfigureAwait(false));
                    if (readResult.NotFound)
                        LogProjectionInstanceNotFound(log, null);

                    return readResult;
                }
                catch (Exception ex) when (ExceptionFilter.True(() => LogProjectionLoadError(log, ex)))
                {
                    return ReadResult<T>.WithError(ex);
                }
            }
        }

        private Task PersistAsync(IBlobId projectionId, ProjectionVersion version, IEvent @event)
        {
            try
            {
                var commit = new ProjectionCommit(projectionId, version, @event);
                return projectionStore.SaveAsync(commit);
            }
            catch (Exception ex) when (ExceptionFilter.True(() => LogProjectionWriteError(log, ex)))
            {
                return Task.FromException(ex);
            }
        }

        private async Task<ReadResult<ProjectionVersionsHandler>> GetProjectionVersionsFromStoreAsync(string projectionName)
        {
            try
            {
                var persistentVersionType = typeof(ProjectionVersionsHandler);
                string projectionVersions_ProjectionName = persistentVersionType.GetContractId();

                ProjectionVersionManagerId versionId = new ProjectionVersionManagerId(projectionName, contextAccessor.CronusContext.Tenant);
                ProjectionVersion persistentVersion = new ProjectionVersion(projectionVersions_ProjectionName, ProjectionStatus.Live, 1, projectionHasher.CalculateHash(persistentVersionType));
                ProjectionStream stream = await LoadProjectionStreamAsync(versionId, persistentVersion).ConfigureAwait(false);
                ProjectionVersionsHandler projectionInstance = new ProjectionVersionsHandler();
                projectionInstance = await stream.RestoreFromHistoryAsync(projectionInstance).ConfigureAwait(false);

                return new ReadResult<ProjectionVersionsHandler>(projectionInstance);
            }
            catch (Exception ex) when (ExceptionFilter.True(() => LogProjectionLoadError(log, ex)))
            {
                return ReadResult<ProjectionVersionsHandler>.WithError(ex.Message);
            }
        }

        private async Task<ProjectionStream> LoadProjectionStreamAsync(IBlobId projectionId, ProjectionVersion version)
        {
            List<ProjectionCommit> projectionCommits = new List<ProjectionCommit>();

            var loadedCommits = projectionStore.LoadAsync(version, projectionId).ConfigureAwait(false);
            await foreach (var commit in loadedCommits)
            {
                projectionCommits.Add(commit);
            }

            ProjectionStream stream = new ProjectionStream(version, projectionId, projectionCommits.Select(c => c.Event));
            return stream;
        }

        private async Task<ProjectionStream> LoadProjectionStreamAsync(IBlobId projectionId, Type projectionType)
        {
            ProjectionVersion liveVersion = await LoadLiveProjectionVersion(projectionType).ConfigureAwait(false);
            if (liveVersion is not null)
                return await LoadProjectionStreamAsync(projectionId, liveVersion).ConfigureAwait(false);

            else
                return ProjectionStream.Empty();
        }

        private async Task<ProjectionVersion> LoadLiveProjectionVersion(Type projectionType)
        {
            string projectionName = projectionType.GetContractId();

            ReadResult<ProjectionVersions> result = await GetProjectionVersionsAsync(projectionName).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                ProjectionVersion liveVersion = result.Data.GetLive();
                if (liveVersion is null)
                {
                    LogProjectionLiveVersionMissing(log, null);
                }

                return liveVersion;
            }
            else if (result.NotFound)
            {
                LogProjectionInstanceNotFound(log, null);
            }
            else if (result.HasError)
            {
                LogProjectionLoadError(log, null);
            }

            return null;
        }

        private bool ShouldSaveEventForVersion(ProjectionVersion version)
        {
            return version.Status == ProjectionStatus.New || version.Status == ProjectionStatus.Fixing || version.Status == ProjectionStatus.Live;
        }
    }
}
