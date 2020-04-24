using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.EventStore;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations
{
    public class AggregateCommitMigrationWorkflow : MigrationWorkflowBase<AggregateCommit, IEnumerable<AggregateCommit>>
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(AggregateCommitMigrationWorkflow));

        public AggregateCommitMigrationWorkflow(IMigration<AggregateCommit, IEnumerable<AggregateCommit>> migration)
            : base(migration) { }

        protected override IEnumerable<AggregateCommit> Run(Execution<AggregateCommit, IEnumerable<AggregateCommit>> context)
        {
            var commit = context.Context;
            var newCommits = new List<AggregateCommit> { commit };
            try
            {
                if (migration.ShouldApply(commit))
                    newCommits = migration.Apply(commit).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => "Error while applying migration");
            }

            foreach (var newCommit in newCommits)
                yield return newCommit;
        }
    }
}
