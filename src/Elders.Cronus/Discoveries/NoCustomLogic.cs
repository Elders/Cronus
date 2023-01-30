using Elders.Cronus.EventStore;
using Elders.Cronus.Migrations;

namespace Elders.Cronus.Discoveries;

public class NoCustomLogic : IMigrationCustomLogic
{
    public void OnAggregateCommit(AggregateCommit migratedAggregateCommit) { }
}
