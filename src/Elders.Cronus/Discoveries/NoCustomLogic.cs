using Elders.Cronus.EventStore;
using Elders.Cronus.Migrations;
using System.Threading.Tasks;

namespace Elders.Cronus.Discoveries;

public class NoCustomLogic : IMigrationCustomLogic
{
    public Task OnAggregateCommitAsync(AggregateCommit migratedAggregateCommit) => Task.CompletedTask;
}
