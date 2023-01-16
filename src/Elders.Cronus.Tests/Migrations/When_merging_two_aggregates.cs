using Elders.Cronus.Migration.Middleware.Tests.TestModel;
using Elders.Cronus.Migration.Middleware.Tests.TestModel.Bar;
using Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo;
using Elders.Cronus.EventStore;
using Machine.Specifications;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Migrations.TestMigration;
using System;

namespace Elders.Cronus.Migrations
{
    [Subject("Migration")]
    public class When_merging_two_aggregates
    {
        Establish context = async () =>
        {
            var eventStore = new TestEventStore();

            migration = new MergeAggregatesMigration(eventStore);
            migrationOuput = new List<AggregateCommit>();
            var fooId = new FooId("1234", "elders");
            aggregateCommitFoo = new AggregateCommit(fooId.RawId, 1, new List<IEvent>
                {
                    new TestCreateEventFoo(fooId),
                    new TestUpdateEventFoo(fooId, string.Empty)
                },
                new List<IPublicEvent>(), DateTimeOffset.Now.ToFileTime());

            await eventStore.AppendAsync(aggregateCommitFoo).ConfigureAwait(false);

            var barId = new BarId("1234", "elders");
            aggregateCommitBar = new AggregateCommit(barId.RawId, 1, new List<IEvent> { new TestCreateEventBar(barId) }, new List<IPublicEvent>(), DateTimeOffset.Now.ToFileTime());
        };

        Because of = () =>
        {
            migrationOuput.AddRange(migration.Apply(aggregateCommitBar).ToList());
        };

        It the_migration_should_return_one_aggegateCommit = () => migrationOuput.Count.ShouldEqual(1);
        It the_migration_should_contain_correnct_number_of_events = () => migrationOuput.SelectMany(x => x.Events).Count().ShouldEqual(1);
        It the_migration_should_contain_only_events_from_new_aggregate =
            () => migrationOuput.Select(x => x.Events.Select(e => e.GetType().GetContractId())).ShouldContain(contracts);

        static IMigration<AggregateCommit, IEnumerable<AggregateCommit>> migration;
        static AggregateCommit aggregateCommitFoo;
        static AggregateCommit aggregateCommitBar;
        static List<AggregateCommit> migrationOuput;

        static List<string> contracts = new List<string>
        {
            typeof(TestCreateEventFoo).GetContractId()
        };
    }
}
