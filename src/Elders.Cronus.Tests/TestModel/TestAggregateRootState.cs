using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestAggregateRootState : AggregateRootState<TestAggregateId>
    {
        public string UpdatableField { get; private set; }

        public override TestAggregateId Id { get; set; }

        public override int Version { get; set; }

        public void When(TestCreateEvent e)
        {
            Id = e.Id;
        }

        public void When(TestUpdateEvent e)
        {
            UpdatableField = e.UpdatedFieldValue;
        }
    }
}
