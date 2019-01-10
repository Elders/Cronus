using System.Collections.Generic;
using Elders.Cronus.EventStore;
using Elders.Cronus.Logging;

namespace Elders.Cronus.Migrations
{
    public class CopyEventStore<TSourceEventStorePlayer, TTargetEventStore> : MigrationRunnerBase<AggregateCommitRaw, TSourceEventStorePlayer, TTargetEventStore>
        where TSourceEventStorePlayer : IEventStorePlayer
        where TTargetEventStore : IEventStore
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(MigrationRunnerBase<,,>));

        public CopyEventStore(TSourceEventStorePlayer source, TTargetEventStore target) : base(source, target) { }

        public override void Run(IEnumerable<IMigration<AggregateCommitRaw>> migrations)
        {
            int counter = 0;
            foreach (var sourceCommit in source.LoadAggregateCommitsRaw(5000))
            {
                if (counter % 10000 == 0) log.Info($"[Migrations] Migrated records: {counter}");

                target.Append(sourceCommit);

                counter++;
            }
        }
    }
}
