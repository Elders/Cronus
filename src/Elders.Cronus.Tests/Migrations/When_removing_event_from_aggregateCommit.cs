using Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo;
using Elders.Cronus.EventStore;
using Machine.Specifications;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Migrations.TestMigration;

namespace Elders.Cronus.Migrations
{
    [Subject("Migration")]
    public class When_removing_event_from_aggregateCommit
    {
        Establish context = () =>
        {
            migration = new RemoveEventMigration();
            var id = new FooId("1234", "elders");
            aggregateCommitFoo = new AggregateCommit(id.RawId, 0, new List<IEvent>
                {
                    new TestCreateEventFoo(id),
                    new TestUpdateEventFoo(id, string.Empty)
                });
        };

        Because of = () => migrationOuput = migration.Apply(aggregateCommitFoo).ToList();

        It the_migration_should_add_new_event = () => migrationOuput.Single().Events.Count.ShouldEqual(1);

        static IMigration<AggregateCommit, IEnumerable<AggregateCommit>> migration;
        static AggregateCommit aggregateCommitFoo;
        static IList<AggregateCommit> migrationOuput;
    }
}
