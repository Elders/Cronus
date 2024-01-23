using System.Runtime.Serialization;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;

namespace Elders.Cronus.Migrations
{
    [DataContract(Name = "2f26cd18-0db8-425f-8ada-5e3bf06a57b5")]
    public sealed class MigrationHandler : IMigrationHandler,
        IAggregateCommitHandle<AggregateCommit>
    {
        private readonly ICronusMigratorManual cronusMigrator;

        public MigrationHandler(ICronusMigratorManual cronusMigrator)
        {
            this.cronusMigrator = cronusMigrator;
        }

        public Task HandleAsync(AggregateCommit aggregateCommit)
        {
            return cronusMigrator.MigrateAsync(aggregateCommit);
        }
    }
}
