namespace Elders.Cronus.Tests.TestModel
{
    public class TestSnapshotUpdateEvent : IEvent
    {
        public TestSnapshotUpdateEvent(TestSnapshotAggregateId id, string updatedFieldValue)
        {
            Id = id;
            UpdatedFieldValue = updatedFieldValue;
        }

        public TestSnapshotAggregateId Id { get; set; }

        public string UpdatedFieldValue { get; set; }
    }
}
