﻿using Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo;
using Elders.Cronus.EventStore;
using Machine.Specifications;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Migrations.TestMigration;

namespace Elders.Cronus.Migrations
{
    [Subject("Migration")]
    public class When_adding_event_to_aggregateCommit
    {
        Establish context = () =>
        {
            migration = new AddEventMigration();
            var id = new FooId("1234", "elders");
            aggregateCommitFoo = new AggregateCommit(id.RawId, 0, new List<IEvent> { new TestCreateEventFoo(id) });
        };

        Because of = () => migrationOuput = migration.Apply(aggregateCommitFoo).ToList();

        It the_migration_should_return_single_commit = () => migrationOuput.Count.ShouldEqual(1);
        It the_migration_should_add_new_event = () => migrationOuput.Single().Events.Count.ShouldEqual(2);

        static IMigration<AggregateCommit, IEnumerable<AggregateCommit>> migration;
        static AggregateCommit aggregateCommitFoo;
        static IList<AggregateCommit> migrationOuput;
    }
}
