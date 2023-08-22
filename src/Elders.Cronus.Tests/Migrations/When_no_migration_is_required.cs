using Elders.Cronus.Migration.Middleware.Tests.TestModel.Bar;
using Elders.Cronus.EventStore;
using Machine.Specifications;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Migrations.TestMigration;
using System;

namespace Elders.Cronus.Migrations
{
    [Subject("Migration")]
    public class When_no_migration_is_required
    {
        Establish context = () =>
        {
            var barId = new BarId("1234", "elders");
            migration = new SimpleMigration();
            aggregateCommitBar = new AggregateCommit(barId.RawId, 1, new List<IEvent> { new TestCreateEventBar(barId) }, new List<IPublicEvent>(), DateTimeOffset.Now.ToFileTime());
        };

        Because of = () => migrationOuput = migration.Apply(aggregateCommitBar).ToList();

        It the_migration_output_should_not_be_null = () => migrationOuput.ShouldNotBeNull();
        It the_migration_should_not_change_aggregateCommit_count = () => migrationOuput.Count.ShouldEqual(1);
        It the_migration_output_should_be_same_as_the_input = () => migrationOuput.ShouldContainOnly(aggregateCommitBar);


        static IMigration<AggregateCommit, IEnumerable<AggregateCommit>> migration;
        static AggregateCommit aggregateCommitBar;
        static IList<AggregateCommit> migrationOuput;
    }
}
