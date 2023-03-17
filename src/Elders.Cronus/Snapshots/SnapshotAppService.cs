using System.Threading.Tasks;

namespace Elders.Cronus.Snapshots
{
    internal sealed class SnapshotAppService : ApplicationService<SnapshotManager>, ISystemAppService,
        ICommandHandler<RequestSnapshot>,
        ICommandHandler<MarkSnapshotAsCreated>,
        ICommandHandler<MarkSnapshotAsCanceled>,
        ICommandHandler<MarkSnapshotCreationAsFailed>
    {
        private readonly ISnapshotStrategy snapshotStrategy;

        public SnapshotAppService(IAggregateRepository repository, ISnapshotStrategy snapshotStrategy) : base(repository)
        {
            this.snapshotStrategy = snapshotStrategy;
        }

        public async Task HandleAsync(RequestSnapshot command)
        {
            var aggregateType = command.AggregareContract.GetTypeByContract();
            if (aggregateType.IsSnapshotable() == false)
                return;

            var loadResult = await repository.LoadAsync<SnapshotManager>(command.Id).ConfigureAwait(false);
            SnapshotManager snapshot;
            if (loadResult.IsSuccess == false)
                snapshot = new SnapshotManager();
            else
                snapshot = loadResult.Data;

            await snapshot.RequestSnapshotAsync(command.Id, command.Revision, command.AggregareContract, snapshotStrategy).ConfigureAwait(false);
            await repository.SaveAsync(snapshot).ConfigureAwait(false);
        }

        public Task HandleAsync(MarkSnapshotAsCreated command)
        {
            return UpdateAsync(command.Id, ar => ar.MarkAsCreated(command.Revision));
        }

        public Task HandleAsync(MarkSnapshotAsCanceled command)
        {
            return UpdateAsync(command.Id, ar => ar.MarkAsCanceled(command.Revision));
        }

        public Task HandleAsync(MarkSnapshotCreationAsFailed command)
        {
            return UpdateAsync(command.Id, ar => ar.MarkAsFailed(command.Revision));
        }
    }
}
