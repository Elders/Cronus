using System.Collections.Generic;
using Elders.Cronus.EventStore;
using Elders.Cronus.Logging;

namespace Elders.Cronus.Migrations
{
    public class MigrationRunner<TSourceEventStorePlayer, TTargetEventStore>
        where TSourceEventStorePlayer : IEventStorePlayer
        where TTargetEventStore : IEventStore
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(MigrationRunner<,>));

        private readonly IEventStorePlayer source;
        private readonly IEventStore target;

        public MigrationRunner(TSourceEventStorePlayer source, TTargetEventStore target)
        {
            this.source = source;
            this.target = target;
        }

        public void Run(IEnumerable<IMigration<AggregateCommit>> migrations)
        {
            int counter = 0;
            foreach (var sourceCommit in source.LoadAggregateCommits(5000))
            {
                if (counter % 10000 == 0) log.Info($"Migrations counter: {counter}");

                AggregateCommit current = sourceCommit;
                foreach (var migration in migrations)
                {
                    current = new OverwriteAggregateCommitMigrationWorkflow(migration).Run(sourceCommit);
                }
                target.Append(current);

                counter++;
            }
        }
    }
}
