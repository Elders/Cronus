using Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo;
using Elders.Cronus.EventStore;
using System.Collections.Generic;
using System.Text;

namespace Elders.Cronus.Migrations.TestMigration
{
    public class AddEventMigration : IMigration<AggregateCommit, IEnumerable<AggregateCommit>>
    {
        readonly string targetAggregateName = "Foo".ToLowerInvariant();
        static readonly FooId id = new FooId("1234", "elders");
        static readonly IEvent eventToAdd = new TestUpdateEventFoo(id, "updated");

        public IEnumerable<AggregateCommit> Apply(AggregateCommit current)
        {
            if (ShouldApply(current))
            {
                var newEvents = new List<IEvent>(current.Events);
                newEvents.Add(eventToAdd);
                var newAggregateCommit = new AggregateCommit(current.AggregateRootId, current.Revision, newEvents);

                yield return newAggregateCommit;
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
