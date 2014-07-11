using Elders.Cronus.DomainModelling;

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
}
