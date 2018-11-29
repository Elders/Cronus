namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo
{
    public class TestAggregateRootStateFoo : AggregateRootState<TestAggregateRootFoo, FooId>
    {
        public string UpdatableField { get; private set; }

        public override FooId Id { get; set; }

        public void When(TestCreateEventFoo e)
        {
            Id = e.Id;
        }

        public void When(TestUpdateEventFoo e)
        {
            UpdatableField = e.UpdatedFieldValue;
        }
    }
}
