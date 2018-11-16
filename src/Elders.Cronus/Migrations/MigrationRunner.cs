using System.Collections.Generic;
using Elders.Cronus.EventStore;
using Elders.Cronus.Logging;

namespace Elders.Cronus.Migrations
{
    public abstract class MigrationRunnerBase<TDataFormat, TSourceEventStorePlayer, TTargetEventStore>
        where TSourceEventStorePlayer : IEventStorePlayer
        where TTargetEventStore : IEventStore
    {
        protected readonly IEventStorePlayer source;
        protected readonly IEventStore target;

        public MigrationRunnerBase(TSourceEventStorePlayer source, TTargetEventStore target)
        {
            this.source = source;
            this.target = target;
        }

        /// <summary>
        /// Applies the specified migrations to every <see cref="TDataFormat"/>
        /// </summary>
        /// <param name="migrations"></param>
        public abstract void Run(IEnumerable<IMigration<TDataFormat>> migrations);

    }

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
