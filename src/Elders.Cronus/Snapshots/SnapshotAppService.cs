﻿using System.Threading.Tasks;
using Elders.Cronus.Snapshots.Options;
using Elders.Cronus.Snapshots.Strategy;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.Snapshots
{
    internal sealed class SnapshotAppService : ApplicationService<SnapshotManager>, ISystemAppService,
        ICommandHandler<RequestSnapshot>,
        ICommandHandler<CompleteSnapshot>,
        ICommandHandler<CancelSnapshot>,
        ICommandHandler<FailSnapshotCreation>
    {
        private readonly ISnapshotStrategy<AggregateSnapshotStrategyContext> snapshotStrategy;
        private readonly SnapshotManagerOptions _options;

        public SnapshotAppService(IAggregateRepository repository, ISnapshotStrategy<AggregateSnapshotStrategyContext> snapshotStrategy, IOptionsMonitor<SnapshotManagerOptions> monitor) : base(repository)
        {
            this.snapshotStrategy = snapshotStrategy;
            _options = monitor.CurrentValue;
        }

        public async Task HandleAsync(RequestSnapshot command)
        {
            var instanceType = command.Contract.GetTypeByContract();
            if (instanceType.IsSnapshotable() == false)
                return;

            var loadResult = await repository.LoadAsync<SnapshotManager>(command.Id).ConfigureAwait(false);
            SnapshotManager snapshot;
            if (loadResult.IsSuccess == false)
                snapshot = new SnapshotManager(command.Id);
            else
                snapshot = loadResult.Data;

            await snapshot.RequestSnapshotAsync(command.Id, command.Revision, command.Contract, command.EventsLoaded, command.LoadTime, snapshotStrategy, _options).ConfigureAwait(false);
            await repository.SaveAsync(snapshot).ConfigureAwait(false);
        }

        public Task HandleAsync(CompleteSnapshot command)
        {
            return UpdateAsync(command.Id, ar => ar.Complete(command.Revision));
        }

        public Task HandleAsync(CancelSnapshot command)
        {
            return UpdateAsync(command.Id, ar => ar.Cancel(command.Revision));
        }

        public Task HandleAsync(FailSnapshotCreation command)
        {
            return UpdateAsync(command.Id, ar => ar.Fail(command.Revision));
        }
    }
}
