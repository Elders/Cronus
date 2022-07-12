using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "28345d27-0ccf-48dc-88dc-2d10bed829cf")]
    public class ProjectionVersionManagerAppService : ApplicationService<ProjectionVersionManager>, ISystemAppService,
        ICommandHandler<RegisterProjection>,
        ICommandHandler<ReplayProjection>,
        ICommandHandler<FinalizeProjectionVersionRequest>,
        ICommandHandler<CancelProjectionVersionRequest>,
        ICommandHandler<TimeoutProjectionVersionRequest>,
        ICommandHandler<RebuildProjectionCommand>
    {
        private readonly IProjectionVersioningPolicy projectionVersioningPolicy;
        private readonly InMemoryProjectionVersionStore projectionStore;

        public ProjectionVersionManagerAppService(IAggregateRepository repository, IProjectionVersioningPolicy projectionVersioningPolicy, InMemoryProjectionVersionStore projectionStore) : base(repository)
        {
            this.projectionVersioningPolicy = projectionVersioningPolicy;
            this.projectionStore = projectionStore;
        }

        public async Task HandleAsync(RegisterProjection command)
        {
            ProjectionVersionManager ar = null;
            ReadResult<ProjectionVersionManager> result = await repository.LoadAsync<ProjectionVersionManager>(command.Id).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                ar = result.Data;
                ar.NotifyHash(command.Id, command.Hash, projectionVersioningPolicy, projectionStore);
            }

            if (result.NotFound)
                ar = new ProjectionVersionManager(command.Id, command.Hash);

            await repository.SaveAsync(ar).ConfigureAwait(false);
        }

        public Task HandleAsync(ReplayProjection command)
        {
            return UpdateAsync(command.Id, ar => ar.Replay(command.Hash, projectionVersioningPolicy));
        }

        public Task HandleAsync(RebuildProjectionCommand command)
        {
            return UpdateAsync(command.Id, ar => ar.Rebuild(command.Hash));
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
    }
}
