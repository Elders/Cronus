namespace Elders.Cronus.Projections.Versioning
{
    public class ProjectionVersionManagerAppService : AggregateRootApplicationService<ProjectionVersionManager>, ISystemService,
        ICommandHandler<RegisterProjection>,
        ICommandHandler<RebuildProjection>,
        ICommandHandler<FinalizeProjectionVersionRequest>,
        ICommandHandler<CancelProjectionVersionRequest>,
        ICommandHandler<TimeoutProjectionVersionRequest>
    {
        public void Handle(RegisterProjection command)
        {
            ProjectionVersionManager ar;
            if (Repository.TryLoad(command.Id, out ar) == false)
            {
                ar = new ProjectionVersionManager(command.Id, command.Hash);
            }
            else
            {
                ar.NotifyHash(command.Hash);
            }

            Repository.Save(ar);
        }

        public void Handle(RebuildProjection command)
        {
            Update(command.Id, ar => ar.Replay(command.Hash));
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
