using System.Collections.Generic;
using Elders.Cronus.EventStore;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations
{
    public class CopyEventStore<TSourceEventStorePlayer, TTargetEventStore> : MigrationRunnerBase<AggregateCommitRaw, TSourceEventStorePlayer, TTargetEventStore>
        where TSourceEventStorePlayer : IEventStorePlayer
        where TTargetEventStore : IEventStore
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(CopyEventStore<,>));

        public CopyEventStore(TSourceEventStorePlayer source, TTargetEventStore target) : base(source, target) { }

        public override void Run(IEnumerable<IMigration<AggregateCommitRaw>> migrations)
        {
            int counter = 0;
            foreach (var sourceCommit in source.LoadAggregateCommitsRaw(5000))
            {
                if (counter % 10000 == 0) logger.Info($"[Migrations] Migrated records: {counter}");

                target.Append(sourceCommit);

                counter++;
            }
        }
    }
}
