using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations
{
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

        public Task MigrateAsync(AggregateCommit aggregateCommit)
        {
            foreach (var migration in migrations)
            {
                if (migration.ShouldApply(aggregateCommit))
                    aggregateCommit = migration.Apply(aggregateCommit);
            }

            try
            {
                return theLogic.OnAggregateCommitAsync(aggregateCommit);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => "We do not trust people that inject their custom logic into important LOOOOONG running processes like this one.");
                return Task.FromException(ex);
            }
        }
    }
}
