using System.Collections.Generic;
using Elders.Cronus.EventStore;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Migrations
{
    public class MigrationRunner<TSourceEventStorePlayer, TTargetEventStore>
        where TSourceEventStorePlayer : IEventStorePlayer
        where TTargetEventStore : IEventStore
    {
        private readonly IEventStorePlayer source;
        private readonly IEventStore target;

        public MigrationRunner(TSourceEventStorePlayer source, TTargetEventStore target)
        {
            this.source = source;
            this.target = target;
        }

        public void Run(IEnumerable<IMigration<AggregateCommit>> migrations)
        {
            foreach (var sourceCommit in source.LoadAggregateCommits(1000))
            {
                AggregateCommit current = sourceCommit;
                foreach (var migration in migrations)
                {
                    current = new OverwriteAggregateCommitMigrationWorkflow(migration).Run(sourceCommit);
                }
                target.Append(current);
            }
        }
    }



    public static class Alabala
    {
        public static IServiceCollection AddMigrationRunner(this IServiceCollection services)
        {


            return services;
        }
    }
}
