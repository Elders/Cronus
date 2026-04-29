using Elders.Cronus.EventStore;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Migrations;

/// <summary>
/// Hook for executing custom logic for every aggregate commit produced during a migration run.
/// </summary>
public interface IMigrationCustomLogic
{
    /// <summary>
    /// Invoked for each migrated <see cref="AggregateCommit"/> so callers can apply additional, custom side effects.
    /// </summary>
    /// <param name="migratedAggregateCommit">The aggregate commit produced by the migrator.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task OnAggregateCommitAsync(AggregateCommit migratedAggregateCommit, CancellationToken cancellationToken = default);
}
