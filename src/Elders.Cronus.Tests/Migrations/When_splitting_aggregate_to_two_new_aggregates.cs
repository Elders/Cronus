using Elders.Cronus.Migration.Middleware.Tests.TestModel.FooBar;
using Elders.Cronus.EventStore;
using Machine.Specifications;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Migrations.TestMigration;
using System;

namespace Elders.Cronus.Migrations
{
    [Subject("Migration")]
    public class When_splitting_aggregate_to_two_new_aggregates
    {
        Establish context = () =>
        {
            migration = new SplitAggregateMigration();
            var fooBarId = new FooBarId("1234", "elders");
            aggregateCommitFooBar = new AggregateCommit(fooBarId.RawId, 1, new List<IEvent>
                {
                    new TestCreateEventFooBar(fooBarId),
                    new TestUpdateEventFooBar(fooBarId, string.Empty)
                }, new List<IPublicEvent>(), DateTimeOffset.Now.ToFileTime());
        };

        Because of = () => migrationOuput = migration.Apply(aggregateCommitFooBar).ToList();

        It the_migration_should_return_two_aggegateCommits = () => migrationOuput.Count.ShouldEqual(2);
        It the_migration_should__not_contain_initial_aggregateCommit = () => migrationOuput.ShouldNotContain(aggregateCommitFooBar);

        static IMigration<AggregateCommit, IEnumerable<AggregateCommit>> migration;
        static AggregateCommit aggregateCommitFooBar;
        static IList<AggregateCommit> migrationOuput;
    }
}
