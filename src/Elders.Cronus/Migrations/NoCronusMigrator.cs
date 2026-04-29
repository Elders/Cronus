using Elders.Cronus.EventStore;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Migrations;

/// <summary>
/// Default migrator that performs no work. Used when no real migrator has been registered.
/// </summary>
public sealed class NoCronusMigrator : ICronusMigrator
{
    /// <inheritdoc />
    public Task MigrateAsync(AggregateCommit aggregateCommit, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
