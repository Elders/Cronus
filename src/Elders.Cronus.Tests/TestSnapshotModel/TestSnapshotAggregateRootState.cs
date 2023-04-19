using System.Collections.Generic;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestSnapshotAggregateRootState : AggregateRootState<TestSnapshotAggregateRoot, TestSnapshotAggregateId>
    {
        public TestSnapshotAggregateRootState()
        {

        }

        public override TestSnapshotAggregateId Id { get; set; }

        public string UpdatableField { get; set; }

        public void When(TestSnapshotCreateEvent e)
        {
            Id = e.Id;
        }

        public void When(TestSnapshotUpdateEvent e)
        {
            UpdatableField = e.UpdatedFieldValue;
        }
    }
}
