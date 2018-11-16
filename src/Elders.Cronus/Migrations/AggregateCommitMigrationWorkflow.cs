using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.EventStore;
using Elders.Cronus.Logging;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.Migrations
{
    public class AggregateCommitMigrationWorkflow : MigrationWorkflowBase<AggregateCommit, IEnumerable<AggregateCommit>>
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(AggregateCommitMigrationWorkflow));

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
                log.ErrorException("Error while applying migration", ex);
            }

            foreach (var newCommit in newCommits)
                yield return newCommit;
        }
    }
}
