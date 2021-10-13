using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.EventStore;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations
{
    public class MigrationRunner<TSourceEventStorePlayer, TTargetEventStore> : MigrationRunnerBase<AggregateCommit, IMigrationEventStorePlayer, IEventStore>
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(MigrationRunner<,>));

        public MigrationRunner(IMigrationEventStorePlayer source, EventStoreFactory factory) : base(source, factory.GetEventStore()) { }

        public override void Run(IEnumerable<IMigration<AggregateCommit>> migrations)
        {
            LoadAggregateCommitsResult result = source.LoadAggregateCommits("", 5000);
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
                        target.Append(sourceCommit);
                    }
                    result = source.LoadAggregateCommits(result.PaginationToken, 5000); // think of paging
                    logger.Info(() => $"Migrated commits - Counter: `{counter}` - Pagination Token: `{result.PaginationToken}`");
                }
            }
            catch (System.Exception ex)
            {
                logger.ErrorException(ex, () => $"Something boom bam while runnning migration. Here is paginator: {result.PaginationToken}");
            }
        }
    }
}
