using Elders.Cronus;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.Migrations;
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract(Name = "2f26cd18-0db8-425f-8ada-5e3bf06a57b5")]
public class MigrationHandler : IMigrationHandler,
    IAggregateCommitHandle<AggregateCommit>
{
    private readonly IEventStore eventStore;
    private readonly IEnumerable<IMigration<AggregateCommit>> migrations;

    public MigrationHandler(IEventStore eventStore, IEnumerable<IMigration<AggregateCommit>> migrations)
    {
        this.eventStore = eventStore;
        this.migrations = migrations;
    }

    public void Handle(AggregateCommit aggregateCommit)
    {
        foreach (var migration in migrations)
        {
            if (migration.ShouldApply(aggregateCommit))
                aggregateCommit = migration.Apply(aggregateCommit);
        }

        eventStore.Append(aggregateCommit);
    }
}
