namespace Elders.Cronus.Tests.TestModel
{
    public class TestCreateEvent : IEvent
    {
        public TestCreateEvent(TestAggregateId id)
        {
            Id = id;
        }

        public TestAggregateId Id { get; set; }
    }

    public class TestCreateEntityEvent : IEvent
    {
        public TestCreateEntityEvent(TestEntityId entityId)
        {
            this.EntityId = entityId;
        }

        public TestEntityId EntityId { get; set; }

    }
}
