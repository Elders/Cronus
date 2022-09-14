using Elders.Cronus.Migration.Middleware.Tests.TestModel.Bar;
using Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo;
using Elders.Cronus.Migration.Middleware.Tests.TestModel.FooBar;
using Elders.Cronus.EventStore;
using System.Collections.Generic;
using System.Text;

namespace Elders.Cronus.Migrations.TestMigration
{
    public class ProduceNewAggregateMigration : IMigration<AggregateCommit, IEnumerable<AggregateCommit>>
    {
        readonly string targetAggregateFoo = "Foo".ToLowerInvariant();
        readonly string targetAggregateBar = "Bar".ToLowerInvariant();
        static readonly FooBarId id = new FooBarId("1234", "elders");
        readonly Dictionary<IAggregateRootId, int> aggregateMaxRevision;

        public ProduceNewAggregateMigration()
        {
            aggregateMaxRevision = new Dictionary<IAggregateRootId, int>();
        }

        public IEnumerable<AggregateCommit> Apply(AggregateCommit current)
        {
            if (ShouldApply(current))
            {
                var urnRaw = Urn.Parse(Encoding.UTF8.GetString(current.AggregateRootId));
                var urn = AggregateUrn.Parse(urnRaw.Value);
                string currentAggregateName = urn.AggregateRootName;

                if (currentAggregateName == targetAggregateFoo)
                {
                    var fooBarId = new FooBarId("1234", "elders");
                    var newFooBarEvents = new List<IEvent>();
                    foreach (IEvent @event in current.Events)
                    {
                        if (@event.GetType() == typeof(TestCreateEventFoo))
                        {
                            newFooBarEvents.Add(new TestCreateEventFooBar(fooBarId));
                        }
                        else if (@event.GetType() == typeof(TestUpdateEventFoo))
                        {
                            var theEvent = @event as TestUpdateEventFoo;
                            newFooBarEvents.Add(new TestUpdateEventFooBar(fooBarId, theEvent.UpdatedFieldValue));
                        }
                    }

                    HandleMaxRevision(fooBarId);
                    var aggregateCommitFooBar = new AggregateCommit(fooBarId.RawId, aggregateMaxRevision[fooBarId], newFooBarEvents);
                    yield return aggregateCommitFooBar;
                }
                else
                {
                    var fooBarId = new FooBarId("1234", "elders");
                    var newFooBarEvents = new List<IEvent>();
                    foreach (IEvent @event in current.Events)
                    {
                        if (@event.GetType() == typeof(TestCreateEventBar))
                        {
                            newFooBarEvents.Add(new TestCreateEventFooBar(fooBarId));
                        }
                        else if (@event.GetType() == typeof(TestUpdateEventBar))
                        {
                            var theEvent = @event as TestUpdateEventBar;
                            newFooBarEvents.Add(new TestUpdateEventFooBar(fooBarId, theEvent.UpdatedFieldValue));
                        }
                    }
                    HandleMaxRevision(fooBarId);
                    var aggregateCommitFooBar = new AggregateCommit(fooBarId.RawId, aggregateMaxRevision[fooBarId], newFooBarEvents);
                    yield return aggregateCommitFooBar;
                }
            }
            else
                yield return current;
        }

        public bool ShouldApply(AggregateCommit current)
        {
            var urnRaw = Urn.Parse(Encoding.UTF8.GetString(current.AggregateRootId));
            var urn = AggregateUrn.Parse(urnRaw.Value);
            string currentAggregateName = urn.AggregateRootName;

            if (currentAggregateName == targetAggregateFoo || currentAggregateName == targetAggregateBar)
                return true;

            return false;
        }

        void HandleMaxRevision(IAggregateRootId rootId)
        {
            if (aggregateMaxRevision.ContainsKey(rootId) == false)
                aggregateMaxRevision.Add(rootId, 1);
            else
                aggregateMaxRevision[rootId]++;
        }

    }
}
