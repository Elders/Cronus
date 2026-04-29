using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;

namespace Elders.Cronus.Migrations;

/// <summary>
/// Migrates an <see cref="AggregateCommit"/> from its current schema to a target schema.
/// </summary>
public interface ICronusMigrator
{
    /// <summary>
    /// Performs the migration for the given <paramref name="aggregateCommit"/>.
    /// </summary>
    /// <param name="aggregateCommit">The aggregate commit to migrate.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task MigrateAsync(AggregateCommit aggregateCommit, CancellationToken cancellationToken = default);
}

/// <summary>
/// Marker for migrators that should be triggered manually (out-of-band) rather than as part of the automatic pipeline.
/// </summary>
public interface ICronusMigratorManual : ICronusMigrator { }
