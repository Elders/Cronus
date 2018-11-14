using System;
using Elders.Cronus.EventStore;
using Elders.Cronus.Logging;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.Migrations
{
    public class OverwriteAggregateCommitMigrationWorkflow : GenericMigrationWorkflow<AggregateCommit, AggregateCommit>
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
}
