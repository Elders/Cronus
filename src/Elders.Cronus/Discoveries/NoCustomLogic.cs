using Elders.Cronus.EventStore;
using Elders.Cronus.Migrations;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Discoveries;

/// <summary>
/// Default migration custom-logic that performs no work.
/// </summary>
public class NoCustomLogic : IMigrationCustomLogic
{
    /// <inheritdoc />
    public Task OnAggregateCommitAsync(AggregateCommit migratedAggregateCommit, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
