using System.Collections.Generic;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations
{
    public class CopyEventStore<TSourceEventStorePlayer, TTargetEventStore> : MigrationRunnerBase<AggregateCommitRaw, TSourceEventStorePlayer, TTargetEventStore>
        where TSourceEventStorePlayer : IEventStorePlayer
        where TTargetEventStore : IEventStore
    {
        private readonly ILogger logger;

        public CopyEventStore(TSourceEventStorePlayer source, TTargetEventStore target, ILogger logger) : base(source, target)
        {
            this.logger = logger;
        }

        public override async Task RunAsync(IEnumerable<IMigration<AggregateCommitRaw>> migrations)
        {
            int counter = 0;
            var arCommits = source.LoadAggregateCommitsRawAsync(5000).ConfigureAwait(false);
            await foreach (AggregateCommitRaw sourceCommit in arCommits)
            {
                if (counter % 10000 == 0) logger.Info(() => $"[Migrations] Migrated records: {counter}");

                await target.AppendAsync(sourceCommit).ConfigureAwait(false);

                counter++;
            }
        }
    }
}
