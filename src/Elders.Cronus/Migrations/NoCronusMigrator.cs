using Elders.Cronus.EventStore;
using System.Threading.Tasks;

namespace Elders.Cronus.Migrations;

public sealed class NoCronusMigrator : ICronusMigrator
{
    public Task MigrateAsync(AggregateCommit aggregateCommit)
    {
        return Task.CompletedTask;
    }
}
