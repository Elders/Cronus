using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations;

public sealed class CronusMigrator : ICronusMigrator, ICronusMigratorManual
{
    private readonly IEnumerable<IMigration<AggregateCommit>> migrations;
    private readonly IMigrationCustomLogic theLogic;
    private readonly ILogger<MigrationHandler> logger;

    public CronusMigrator(IEnumerable<IMigration<AggregateCommit>> migrations, IMigrationCustomLogic theLogic, ILogger<MigrationHandler> logger)
    {
        this.migrations = migrations;
        this.theLogic = theLogic;
        this.logger = logger;
    }

    public async Task MigrateAsync(AggregateCommit aggregateCommit)
    {
        foreach (var migration in migrations)
        {
            if (migration.ShouldApply(aggregateCommit))
                aggregateCommit = migration.Apply(aggregateCommit);
        }

        try
        {
            await theLogic.OnAggregateCommitAsync(aggregateCommit).ConfigureAwait(false);
        }
        catch (Exception ex) when (False(() => logger.LogError(ex, $"IMigrationCustomLogic has failed. ARID: {Encoding.UTF8.GetString(aggregateCommit.AggregateRootId.Span)} Rev: {aggregateCommit.Revision}")))
        {
        }
    }
}
