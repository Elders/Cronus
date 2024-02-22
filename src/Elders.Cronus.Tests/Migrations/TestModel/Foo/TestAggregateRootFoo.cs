namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo;

public class TestAggregateRootFoo : AggregateRoot<TestAggregateRootStateFoo>
{
    TestAggregateRootFoo() { }
    public TestAggregateRootFoo(FooId id)
    {
        var @event = new TestCreateEventFoo(id);
        Apply(@event);
    }

    public void Update(string text)
    {
        var @event = new TestUpdateEventFoo(state.Id, text);
        Apply(@event);
    }

    public TestAggregateRootStateFoo State { get { return base.state; } }
}
