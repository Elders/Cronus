using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;

namespace Elders.Cronus.Migrations;

/// <summary>
/// Aggregate-commit handler that delegates each commit to the registered <see cref="ICronusMigrator"/>.
/// </summary>
[DataContract(Name = "2f26cd18-0db8-425f-8ada-5e3bf06a57b5")]
public sealed class MigrationHandler : IMigrationHandler,
    IAggregateCommitHandle<AggregateCommit>
{
    private readonly ICronusMigrator cronusMigrator;

    public MigrationHandler(ICronusMigrator cronusMigrator)
    {
        this.cronusMigrator = cronusMigrator;
    }

    /// <inheritdoc />
    public Task HandleAsync(AggregateCommit aggregateCommit, CancellationToken cancellationToken = default)
    {
        return cronusMigrator.MigrateAsync(aggregateCommit, cancellationToken);
    }
}
