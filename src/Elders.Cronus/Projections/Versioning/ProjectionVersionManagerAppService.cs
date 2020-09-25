﻿namespace Elders.Cronus.Projections.Versioning
{
    public class ProjectionVersionManagerAppService : ApplicationService<ProjectionVersionManager>, ISystemService,
        ICommandHandler<RegisterProjection>,
        ICommandHandler<RebuildProjection>,
        ICommandHandler<FinalizeProjectionVersionRequest>,
        ICommandHandler<CancelProjectionVersionRequest>,
        ICommandHandler<TimeoutProjectionVersionRequest>
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

        public void Handle(RebuildProjection command)
        {
            Update(command.Id, ar => ar.Rebuild(command.Hash, projectionVersioningPolicy));
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
