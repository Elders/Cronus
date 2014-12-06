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

        public void Update(string text)
        {
            var @event = new TestUpdateEvent(state.Id, text);
            Apply(@event);
        }

        public TestAggregateRootState State { get { return base.state; } }
    }
}
