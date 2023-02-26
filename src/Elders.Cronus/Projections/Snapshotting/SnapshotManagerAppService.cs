using Elders.Cronus.Projections.Snapshotting;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "5e5b75e9-9775-41f7-8309-d02e1f0d8b53")]
    public class SnapshotManagerAppService : ApplicationService<SnapshotManager>, ISystemAppService,
        ICommandHandler<CreateSnapshot>
    {
        public SnapshotManagerAppService(IAggregateRepository repository) : base(repository) { }

        public async Task HandleAsync(CreateSnapshot command)
        {
            SnapshotManager ar = null;
            ReadResult<SnapshotManager> snapshotResult = await repository.LoadAsync<SnapshotManager>(command.SnapshotId).ConfigureAwait(false);
            if (snapshotResult.IsSuccess)
            {
                ar = snapshotResult.Data;
                ar.CreateSnapshot(command.SnapshotId, command.Snapshot);
            }

            if (snapshotResult.NotFound)
                ar = new SnapshotManager(command.SnapshotId, command.Snapshot);

            await repository.SaveAsync(ar).ConfigureAwait(false);
        }
    }
}
