namespace Elders.Cronus.Tests.TestModel
{
    public class TestSnapshotCreateEvent : IEvent
    {
        public TestSnapshotCreateEvent(TestSnapshotAggregateId id)
        {
            Id = id;
        }

        public TestSnapshotAggregateId Id { get; set; }
    }
}
