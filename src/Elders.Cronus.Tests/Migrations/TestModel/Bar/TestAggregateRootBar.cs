namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.Bar;

public class TestAggregateRootBar : AggregateRoot<TestAggregateRootStateBar>
{
    TestAggregateRootBar() { }
    public TestAggregateRootBar(BarId id)
    {
        var @event = new TestCreateEventBar(id);
        Apply(@event);
    }

    public void Update(string text)
    {
        var @event = new TestUpdateEventBar(state.Id, text);
        Apply(@event);
    }

    public TestAggregateRootStateBar State { get { return base.state; } }
}
