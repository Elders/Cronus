using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations
{
    public class AggregateCommitMigrationWorkflow : MigrationWorkflowBase<AggregateCommit, IEnumerable<AggregateCommit>>
    {
        private readonly ILogger<AggregateCommitMigrationWorkflow> logger;

        public AggregateCommitMigrationWorkflow(IMigration<AggregateCommit, IEnumerable<AggregateCommit>> migration, ILogger<AggregateCommitMigrationWorkflow> logger)
            : base(migration)
        {
            this.logger = logger;
        }

        protected override Task<IEnumerable<AggregateCommit>> RunAsync(Execution<AggregateCommit, IEnumerable<AggregateCommit>> context)
        {
            var commit = context.Context;
            IEnumerable<AggregateCommit> newCommits = new List<AggregateCommit> { commit };
            try
            {
                if (migration.ShouldApply(commit))
                    newCommits = migration.Apply(commit).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => "Error while applying migration");
            }

           return Task.FromResult(newCommits);
        }
    }
}
