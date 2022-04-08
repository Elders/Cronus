using Elders.Cronus.Migration.Middleware.Tests.TestModel.Bar;
using Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo;
using Elders.Cronus.EventStore;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elders.Cronus.Migrations.TestMigration
{
    public class MergeAggregatesMigration : IMigration<AggregateCommit, IEnumerable<AggregateCommit>>
    {
        readonly string targetAggregateBar = "Bar".ToLowerInvariant();
        readonly Dictionary<IAggregateRootId, int> aggregateMaxRevision;
        readonly IEventStore eventStore;

        public MergeAggregatesMigration(IEventStore eventStore)
        {
            if (ReferenceEquals(eventStore, null) == true) throw new System.ArgumentNullException(nameof(eventStore));
            this.eventStore = eventStore;

            aggregateMaxRevision = new Dictionary<IAggregateRootId, int>();
        }

        private void LoadFromEventStore(IAggregateRootId rootId)
        {
            if (aggregateMaxRevision.ContainsKey(rootId)) return;

            var stream = eventStore.LoadAsync(rootId).GetAwaiter().GetResult();
            if (ReferenceEquals(stream, null) == true)
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
                var urnRaw = Urn.Parse(Encoding.UTF8.GetString(current.AggregateRootId));
                var urn = AggregateUrn.Parse(urnRaw.Value);
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
                var aggregateCommitFooBar = new AggregateCommit(fooId.RawId, aggregateMaxRevision[fooId], newFooEvents);
                yield return aggregateCommitFooBar;

            }
            else
                yield return current;
        }

        public bool ShouldApply(AggregateCommit current)
        {
            var urnRaw = Urn.Parse(Encoding.UTF8.GetString(current.AggregateRootId));
            var urn = AggregateUrn.Parse(urnRaw.Value);
            string currentAggregateName = urn.AggregateRootName;

            if (currentAggregateName == targetAggregateBar)
                return true;

            return false;
        }
    }
}
