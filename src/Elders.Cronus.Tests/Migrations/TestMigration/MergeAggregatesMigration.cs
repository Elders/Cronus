using Elders.Cronus.Migration.Middleware.Tests.TestModel.Bar;
using Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo;
using Elders.Cronus.EventStore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Elders.Cronus.Migrations.TestMigration
{
    public class MergeAggregatesMigration : IMigration<AggregateCommit, IEnumerable<AggregateCommit>>
    {
        readonly string targetAggregateBar = "Bar".ToLowerInvariant();
        readonly Dictionary<AggregateRootId, int> aggregateMaxRevision;
        readonly IEventStore eventStore;

        public MergeAggregatesMigration(IEventStore eventStore)
        {
            if (eventStore is null == true) throw new System.ArgumentNullException(nameof(eventStore));
            this.eventStore = eventStore;

            aggregateMaxRevision = new Dictionary<AggregateRootId, int>();
        }

        private void LoadFromEventStore(AggregateRootId rootId)
        {
            if (aggregateMaxRevision.ContainsKey(rootId)) return;

            var stream = eventStore.LoadAsync(rootId).GetAwaiter().GetResult();
            if (stream is null == true)
            {
                aggregateMaxRevision.Add(rootId, 0);
            }
            else
            {
                var count = stream.Commits.Max(c => c.Revision);
                aggregateMaxRevision.Add(rootId, count);
            }
        }

        public IEnumerable<AggregateCommit> Apply(AggregateCommit current)
        {
            if (ShouldApply(current))
            {
                var urnRaw = new Urn(Encoding.UTF8.GetString(current.AggregateRootId));
                var urn = AggregateRootId.Parse(urnRaw.Value);
                var fooId = new FooId(urn.Id, urn.Tenant);
                LoadFromEventStore(fooId);
                aggregateMaxRevision[fooId]++;

                var newFooEvents = new List<IEvent>();
                foreach (IEvent @event in current.Events)
                {
                    if (@event.GetType() == typeof(TestCreateEventBar))
                    {
                        newFooEvents.Add(new TestCreateEventFoo(fooId));
                    }
                    else if (@event.GetType() == typeof(TestUpdateEventBar))
                    {
                        var theEvent = @event as TestUpdateEventBar;
                        newFooEvents.Add(new TestUpdateEventFoo(fooId, theEvent.UpdatedFieldValue));
                    }
                }
                var aggregateCommitFooBar = new AggregateCommit(fooId.RawId, aggregateMaxRevision[fooId], newFooEvents, new List<IPublicEvent>(), DateTimeOffset.Now.ToFileTime());
                yield return aggregateCommitFooBar;

            }
            else
                yield return current;
        }

        public bool ShouldApply(AggregateCommit current)
        {
            var urnRaw = new Urn(Encoding.UTF8.GetString(current.AggregateRootId));
            var urn = AggregateRootId.Parse(urnRaw.Value);
            string currentAggregateName = urn.AggregateRootName;

            if (currentAggregateName == targetAggregateBar)
                return true;

            return false;
        }
    }
}
