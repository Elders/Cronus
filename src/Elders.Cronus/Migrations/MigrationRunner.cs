using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations
{
    public class MigrationRunner<TSourceEventStorePlayer, TTargetEventStore> : MigrationRunnerBase<AggregateCommit, TSourceEventStorePlayer, IEventStore>
        where TSourceEventStorePlayer: IMigrationEventStorePlayer
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(MigrationRunner<,>));

        public MigrationRunner(TSourceEventStorePlayer source, EventStoreFactory factory) : base(source, factory.GetEventStore()) { }

        public override async Task RunAsync(IEnumerable<IMigration<AggregateCommit>> migrations)
        {
            var data = source.LoadAggregateCommitsAsync(5000).ConfigureAwait(false);


            try
            {
                uint counter = 0;

                await foreach (AggregateCommit sourceCommit in data)
                {
                    counter++;

                    AggregateCommit migrated = sourceCommit;
                    foreach (var migration in migrations)
                    {
                        if (migration.ShouldApply(sourceCommit))
                        {
                            migrated = migration.Apply(sourceCommit);
                        }
                    }

                    if (ForSomeReasonTheAggregateCommitHasBeenDeleted(migrated))
                    {
                        //logger.Debug(() => $"An aggregate commit has been wiped from the face of the Earth. R.I.P.{Environment.NewLine}{result.Commits[i].ToString()}"); // Bonus: How Пикасо is spelled in English => Piccasso, Picasso, Piccaso ?? I bet spmeone will git-blame me
                        continue;
                    }

                    await target.AppendAsync(migrated).ConfigureAwait(false);
                }

               // logger.Info(() => $"Migrated commits - Counter: `{counter}` - Pagination Token: `{result.PaginationToken}`");
            }

            catch (System.Exception ex)
            {
                //logger.ErrorException(ex, () => $"Something boom bam while runnning migration. Here is paginator: {result.PaginationToken}");
            }

        }
        private static bool ForSomeReasonTheAggregateCommitHasBeenDeleted(AggregateCommit aggregateCommit)
        {
            return aggregateCommit is null || aggregateCommit.Events.Any() == false;
        }
    }
}
