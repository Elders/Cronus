using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "28345d27-0ccf-48dc-88dc-2d10bed829cf")]
    public class ProjectionVersionManagerAppService : ApplicationService<ProjectionVersionManager>, ISystemAppService,
        ICommandHandler<RegisterProjection>,
        ICommandHandler<NewProjectionVersion>,
        ICommandHandler<FinalizeProjectionVersionRequest>,
        ICommandHandler<CancelProjectionVersionRequest>,
        ICommandHandler<TimeoutProjectionVersionRequest>,
        ICommandHandler<FixProjectionVersion>,
        ICommandHandler<PauseProjectionVersion>
    {
        private readonly IProjectionVersioningPolicy projectionVersioningPolicy;
        private readonly IProjectionReader projectionReader;

        public ProjectionVersionManagerAppService(IAggregateRepository repository, IProjectionVersioningPolicy projectionVersioningPolicy, IProjectionReader projectionReader) : base(repository)
        {
            this.projectionVersioningPolicy = projectionVersioningPolicy;
            this.projectionReader = projectionReader;
        }

        public async Task HandleAsync(RegisterProjection command)
        {
            ProjectionVersionManager ar = null;
            ReadResult<ProjectionVersionManager> result = await repository.LoadAsync<ProjectionVersionManager>(command.Id).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                ar = result.Data;
                ar.NotifyHash(command.Hash, projectionVersioningPolicy, command.ReplayEventsOptions);

                if (await ShouldRebuildMissingSystemProjectionsAsync(command.Id, projectionReader).ConfigureAwait(false))
                {
                    ar.Rebuild(command.Hash, projectionVersioningPolicy, command.ReplayEventsOptions);
                }
            }

            if (result.NotFound)
                ar = new ProjectionVersionManager(command.Id, command.Hash);

            await repository.SaveAsync(ar).ConfigureAwait(false);
        }

        public Task HandleAsync(NewProjectionVersion command)
        {
            return UpdateAsync(command.Id, ar => ar.Replay(command.Hash, projectionVersioningPolicy, command.ReplayEventsOptions));
        }

        public Task HandleAsync(FixProjectionVersion command)
        {
            return UpdateAsync(command.Id, ar => ar.Rebuild(command.Hash, projectionVersioningPolicy, command.ReplayEventsOptions));
        }

        public Task HandleAsync(FinalizeProjectionVersionRequest command)
        {
            return UpdateAsync(command.Id, ar => ar.FinalizeVersionRequest(command.Version));
        }

        public Task HandleAsync(CancelProjectionVersionRequest command)
        {
            return UpdateAsync(command.Id, ar => ar.CancelVersionRequest(command.Version, command.Reason));
        }

        public Task HandleAsync(TimeoutProjectionVersionRequest command)
        {
            return UpdateAsync(command.Id, ar => ar.VersionRequestTimedout(command.Version, command.Timebox));
        }

        public Task HandleAsync(PauseProjectionVersion command)
        {
            return UpdateAsync(command.Id, ar => ar.PauseVersionRequest(command.Version));
        }

        private async Task<bool> ShouldRebuildMissingSystemProjectionsAsync(ProjectionVersionManagerId projectionId, IProjectionReader projectionReader)
        {
            string projectionName = projectionId.Id;

            if (projectionName.IsProjectionVersionHandler() || projectionName.IsEventStoreIndexStatus())
            {
                ReadResult<ProjectionVersionsHandler> result = await projectionReader.GetAsync<ProjectionVersionsHandler>(projectionId).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    ProjectionVersions versions = result.Data.State.AllVersions;
                    return versions.HasLiveVersion == false && versions.HasRebuildingVersion() == false;
                }
                return true;
            }

            return false;
        }
    }
}
