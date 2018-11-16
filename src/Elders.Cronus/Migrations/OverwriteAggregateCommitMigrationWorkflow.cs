using System;
using Elders.Cronus.EventStore;
using Elders.Cronus.Logging;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.Migrations
{
    public class OverwriteAggregateCommitMigrationWorkflow : MigrationWorkflowBase<AggregateCommit, AggregateCommit>
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(OverwriteAggregateCommitMigrationWorkflow));

        public OverwriteAggregateCommitMigrationWorkflow(IMigration<AggregateCommit, AggregateCommit> migration)
            : base(migration)
        {
        }

        protected override AggregateCommit Run(Execution<AggregateCommit, AggregateCommit> context)
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
                log.ErrorException("Error while applying migration", ex);
            }

            return result;
        }
    }

    public class CopyAggregateCommitMigrationWorkflow : MigrationWorkflowBase<AggregateCommitRaw, AggregateCommitRaw>
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(OverwriteAggregateCommitMigrationWorkflow));

        public CopyAggregateCommitMigrationWorkflow(IMigration<AggregateCommitRaw, AggregateCommitRaw> migration)
            : base(migration)
        {
        }

        protected override AggregateCommitRaw Run(Execution<AggregateCommitRaw, AggregateCommitRaw> context)
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
                log.ErrorException("Error while applying migration", ex);
            }

            return result;
        }
    }
}
