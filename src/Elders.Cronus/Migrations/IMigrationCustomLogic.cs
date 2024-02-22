using Elders.Cronus.EventStore;
using System.Threading.Tasks;

namespace Elders.Cronus.Migrations;

public interface IMigrationCustomLogic
{
    Task OnAggregateCommitAsync(AggregateCommit migratedAggregateCommit);
}
