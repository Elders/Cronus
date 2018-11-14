﻿using Elders.Cronus.Migration.Middleware.Tests.TestModel.Bar;
using Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo;
using Elders.Cronus.Migration.Middleware.Tests.TestModel.FooBar;
using Elders.Cronus.EventStore;
using System.Collections.Generic;
using System.Text;

namespace Elders.Cronus.Migrations.TestMigration
{
    public class SplitAggregateMigration : IMigration<AggregateCommit, IEnumerable<AggregateCommit>>
    {
        readonly string targetAggregateName = "FooBar".ToLowerInvariant();
        static readonly FooBarId id = new FooBarId("1234", "elders");

        public IEnumerable<AggregateCommit> Apply(AggregateCommit current)
        {
            if (ShouldApply(current))
            {
                var fooId = new FooId("1234", "elders");
                var newFooEvents = new List<IEvent>();
                foreach (IEvent @event in current.Events)
                {
                    if (@event.GetType() == typeof(TestCreateEventFooBar))
                    {
                        newFooEvents.Add(new TestCreateEventFoo(fooId));
                    }
                    else if (@event.GetType() == typeof(TestUpdateEventFooBar))
                    {
                        var theEvent = @event as TestUpdateEventFooBar;
                        newFooEvents.Add(new TestUpdateEventFoo(fooId, theEvent.UpdatedFieldValue));
                    }
                }
                var aggregateCommitFoo = new AggregateCommit(fooId.RawId, current.Revision, newFooEvents);
                yield return aggregateCommitFoo;

                var barId = new BarId("5432", "elders");
                var newBarEvents = new List<IEvent>();
                foreach (IEvent @event in current.Events)
                {
                    if (@event.GetType() == typeof(TestCreateEventFooBar))
                    {
                        newBarEvents.Add(new TestCreateEventBar(barId));
                    }
                    else if (@event.GetType() == typeof(TestUpdateEventFooBar))
                    {
                        var theEvent = @event as TestUpdateEventFooBar;
                        newBarEvents.Add(new TestUpdateEventBar(barId, theEvent.UpdatedFieldValue));
                    }
                }
                var aggregateCommitBar = new AggregateCommit(barId.RawId, current.Revision, newFooEvents);

                yield return aggregateCommitBar;

            }
            else
                yield return current;
        }

        public bool ShouldApply(AggregateCommit current)
        {
            var urnRaw = Urn.Parse(Encoding.UTF8.GetString(current.AggregateRootId));
            var urn = StringTenantUrn.Parse(urnRaw.Value);
            string currentAggregateName = urn.ArName;

            if (currentAggregateName == targetAggregateName)
                return true;

            return false;
        }
    }
}
