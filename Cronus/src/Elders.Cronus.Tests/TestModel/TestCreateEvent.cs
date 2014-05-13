using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestCreateEvent : Event
    {
        public TestCreateEvent(TestAggregateId id)
            : base(id)
        {
            Id = id;
        }

        public TestAggregateId Id { get; set; }
    }
}
