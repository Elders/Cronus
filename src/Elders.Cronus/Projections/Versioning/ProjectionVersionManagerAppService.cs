using System.Runtime.Serialization;

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

        public ProjectionVersionManagerAppService(IAggregateRepository repository, IProjectionVersioningPolicy projectionVersioningPolicy) : base(repository)
        {
            this.projectionVersioningPolicy = projectionVersioningPolicy;
        }

        public void Handle(RegisterProjection command)
        {
            ProjectionVersionManager ar = null;
            ReadResult<ProjectionVersionManager> result = repository.Load<ProjectionVersionManager>(command.Id);
            if (result.IsSuccess)
            {
                ar = result.Data;
                ar.NotifyHash(command.Hash, projectionVersioningPolicy);
            }

            if (result.NotFound)
                ar = new ProjectionVersionManager(command.Id, command.Hash);

            repository.Save(ar);
        }

        public void Handle(ReplayProjection command)
        {
            Update(command.Id, ar => ar.Replay(command.Hash, projectionVersioningPolicy));
        }

        public void Handle(RebuildProjectionCommand command)
        {
            Update(command.Id, ar => ar.Rebuild(command.Hash));
        }

        public void Handle(FinalizeProjectionVersionRequest command)
        {
            Update(command.Id, ar => ar.FinalizeVersionRequest(command.Version));
        }

        public void Handle(CancelProjectionVersionRequest command)
        {
            Update(command.Id, ar => ar.CancelVersionRequest(command.Version, command.Reason));
        }

        public void Handle(TimeoutProjectionVersionRequest command)
        {
            Update(command.Id, ar => ar.VersionRequestTimedout(command.Version, command.Timebox));
        }
    }
}
