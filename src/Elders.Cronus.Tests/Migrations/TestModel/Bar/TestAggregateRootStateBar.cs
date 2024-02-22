namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.Bar;

public class TestAggregateRootStateBar : AggregateRootState<TestAggregateRootBar, BarId>
{
    public string UpdatableField { get; private set; }

    public override BarId Id { get; set; }

    public void When(TestCreateEventBar e)
    {
        Id = e.Id;
    }

    public void When(TestUpdateEventBar e)
    {
        UpdatableField = e.UpdatedFieldValue;
    }
}
