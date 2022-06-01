using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations
{
    public sealed class MigrationRunner : MigrationRunnerBase<AggregateCommit, IMigrationEventStorePlayer, IEventStore>
    {
        private readonly ILogger logger;

        public MigrationRunner(IMigrationEventStorePlayer source, EventStoreFactory factory, ILogger logger) : base(source, factory.GetEventStore())
        {
            this.logger = logger;
        }

        public override async Task RunAsync(IEnumerable<IMigration<AggregateCommit>> migrations)
        {
            LoadAggregateCommitsResult result = await source.LoadAggregateCommitsAsync(string.Empty, 5000).ConfigureAwait(false);
            int counter = 0;
            try
            {
                while (result.Commits.Any())
                {
                    for (int i = 0; i < result.Commits.Count; i++)
                    {
                        counter++;
                        AggregateCommit sourceCommit = result.Commits[i];

                        foreach (var migration in migrations)
                        {
                            if (migration.ShouldApply(sourceCommit))
                            {
                                sourceCommit = migration.Apply(sourceCommit);
                            }
                        }

                        if (ForSomeReasonTheAggregateCommitHasBeenDeleted(sourceCommit))
                        {
                            logger.Debug(() => $"An aggregate commit has been wiped from the face of the Earth. R.I.P.{Environment.NewLine}{result.Commits[i].ToString()}"); // Bonus: How Пикасо is spelled in English => Piccasso, Picasso, Piccaso ?? I bet spmeone will git-blame me
                            continue;
                        }

                        await target.AppendAsync(sourceCommit).ConfigureAwait(false);

                    }
                    result = await source.LoadAggregateCommitsAsync(result.PaginationToken, 5000).ConfigureAwait(false); // think of paging
                    logger.Info(() => $"Migrated commits - Counter: `{counter}` - Pagination Token: `{result.PaginationToken}`");
                }
            }
            catch (System.Exception ex)
            {
                logger.ErrorException(ex, () => $"Something boom bam while runnning migration. Here is paginator: {result.PaginationToken}");
            }
        }

        private static bool ForSomeReasonTheAggregateCommitHasBeenDeleted(AggregateCommit aggregateCommit)
        {
            return aggregateCommit is null || aggregateCommit.Events.Any() == false;
        }
    }
}
