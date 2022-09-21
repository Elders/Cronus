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
            var data = source.LoadAggregateCommitsAsync().ConfigureAwait(false);

            try
            {
                List<Task> tasks = new List<Task>();
                await foreach (AggregateCommit sourceCommit in data)
                {
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
                        logger.Debug(() => $"An aggregate commit has been wiped from the face of the Earth. R.I.P."); // Bonus: How Пикасо is spelled in English => Piccasso, Picasso, Piccaso ?? I bet spmeone will git-blame me
                        continue;
                    }

                    var task = target.AppendAsync(migrated);
                    tasks.Add(task);

                    if (tasks.Count > 100)
                    {
                        Task finished = await Task.WhenAny(tasks).ConfigureAwait(false);
                        tasks.Remove(finished);
                    }
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);

                // logger.Info(() => $"Migrated commits - Counter: `{counter}` - Pagination Token: `{result.PaginationToken}`");
            }

            catch (System.Exception ex)
            {
                logger.ErrorException(ex, () => $"Something boom bam while runnning migration.");
            }

        }
        private static bool ForSomeReasonTheAggregateCommitHasBeenDeleted(AggregateCommit aggregateCommit)
        {
            return aggregateCommit is null || aggregateCommit.Events.Any() == false;
        }
    }
}
