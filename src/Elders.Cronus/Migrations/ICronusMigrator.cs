using System.Threading.Tasks;
using Elders.Cronus.EventStore;

namespace Elders.Cronus.Migrations;

public interface ICronusMigrator
{
    Task MigrateAsync(AggregateCommit aggregateCommit);
}

public interface ICronusMigratorManual : ICronusMigrator { }
