namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.FooBar
{
    public class TestAggregateRootStateFooBar : AggregateRootState<TestAggregateRootFooBar, FooBarId>
    {
        public string UpdatableField { get; private set; }

        public override FooBarId Id { get; set; }

        public void When(TestCreateEventFooBar e)
        {
            Id = e.Id;
        }

        public void When(TestUpdateEventFooBar e)
        {
            UpdatableField = e.UpdatedFieldValue;
        }
    }
}
