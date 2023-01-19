using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.Migrations
{
    [DataContract(Name = "2f26cd18-0db8-425f-8ada-5e3bf06a57b5")]
    public class MigrationHandler : IMigrationHandler,
        IAggregateCommitHandle<AggregateCommit>
    {
        private readonly IEventStore eventStore;
        private readonly IEnumerable<IMigration<AggregateCommit>> migrations;
        private readonly IMigrationCustomLogic theLogic;
        private readonly ILogger<MigrationHandler> logger;

        public MigrationHandler(EventStoreFactory eventStoreFactory, IEnumerable<IMigration<AggregateCommit>> migrations, IMigrationCustomLogic theLogic, ILogger<MigrationHandler> logger)
        {
            eventStore = eventStoreFactory.GetEventStore();
            this.migrations = migrations;
            this.theLogic = theLogic;
            this.logger = logger;
        }

        public async Task HandleAsync(AggregateCommit aggregateCommit)
        {
            foreach (var migration in migrations)
            {
                if (migration.ShouldApply(aggregateCommit))
                    aggregateCommit = migration.Apply(aggregateCommit);
            }

            await eventStore.AppendAsync(aggregateCommit).ConfigureAwait(false);

            try
            {
                theLogic.OnAggregateCommit(aggregateCommit);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => "We do not trust people that inject their custom logic into important LOOOOONG running processes like this one.");
            }
        }
    }
    public interface IMigrationCustomLogic
    {
        void OnAggregateCommit(AggregateCommit migratedAggregateCommit);
    }
}
