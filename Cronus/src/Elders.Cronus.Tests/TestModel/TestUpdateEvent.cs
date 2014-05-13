using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestUpdateEvent : Event
    {
        public TestUpdateEvent(TestAggregateId id, string updatedFieldValue)
            : base(id)
        {
            Id = id;
            UpdatedFieldValue = updatedFieldValue;
        }

        public TestAggregateId Id { get; set; }

        public string UpdatedFieldValue { get; set; }
    }
}
