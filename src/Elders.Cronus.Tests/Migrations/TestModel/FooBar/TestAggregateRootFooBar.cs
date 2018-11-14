namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.FooBar
{
    public class TestAggregateRootFooBar : AggregateRoot<TestAggregateRootStateFooBar>
    {
        TestAggregateRootFooBar() { }
        public TestAggregateRootFooBar(FooBarId id)
        {
            var @event = new TestCreateEventFooBar(id);
            Apply(@event);
        }

        public void Update(string text)
        {
            var @event = new TestUpdateEventFooBar(state.Id, text);
            Apply(@event);
        }

        public TestAggregateRootStateFooBar State { get { return base.state; } }
    }
}
