using System;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations
{
    public class OverwriteAggregateCommitMigrationWorkflow : MigrationWorkflowBase<AggregateCommit, AggregateCommit>
    {
        static readonly ILogger logger = CronusLogger.CreateLogger(typeof(OverwriteAggregateCommitMigrationWorkflow));

        public OverwriteAggregateCommitMigrationWorkflow(IMigration<AggregateCommit, AggregateCommit> migration)
            : base(migration)
        {
        }

        protected override Task<AggregateCommit> RunAsync(Execution<AggregateCommit, AggregateCommit> context)
        {
            AggregateCommit result = context.Context;
            var commit = context.Context;
            try
            {
                if (migration.ShouldApply(commit))
                    result = migration.Apply(commit);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => "Error while applying migration");
            }

            return Task.FromResult(result);
        }
    }

    public class CopyAggregateCommitMigrationWorkflow : MigrationWorkflowBase<AggregateCommitRaw, AggregateCommitRaw>
    {
        static readonly ILogger logger = CronusLogger.CreateLogger(typeof(CopyAggregateCommitMigrationWorkflow));

        public CopyAggregateCommitMigrationWorkflow(IMigration<AggregateCommitRaw, AggregateCommitRaw> migration)
            : base(migration)
        {
        }
         
        protected override Task<AggregateCommitRaw> RunAsync(Execution<AggregateCommitRaw, AggregateCommitRaw> context)
        {
            AggregateCommitRaw result = context.Context;
            var commit = context.Context;
            try
            {
                if (migration.ShouldApply(commit))
                    result = migration.Apply(commit);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => "Error while applying migration");
            }

            return Task.FromResult(result);
        }
    }
}
