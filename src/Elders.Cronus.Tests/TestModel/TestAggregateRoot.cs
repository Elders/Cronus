using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestAggregateRoot : AggregateRoot<TestAggregateRootState>
    {
        TestAggregateRoot() { }
        public TestAggregateRoot(TestAggregateId id)
        {
            state = new TestAggregateRootState();

            var @event = new TestCreateEvent(id);
            Apply(@event);
        }
    }
}
