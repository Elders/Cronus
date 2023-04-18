using System;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestSnapshotAggregateRoot : AggregateRoot<TestSnapshotAggregateRootState>, IAmSnapshotable<TestSnapshotState>
    {
        public TestSnapshotAggregateRoot() { }

        public TestSnapshotAggregateRoot(TestSnapshotAggregateId id)
        {
            var @event = new TestSnapshotCreateEvent(id);
            Apply(@event);
        }

        public void DoSomething(string text)
        {
            var @event = new TestSnapshotUpdateEvent(state.Id, text);
            Apply(@event);
        }

        public TestSnapshotState CreateSnapshot()
        {
            return new TestSnapshotState()
            {
                Id = state.Id,
                UpdatableField = state.UpdatableField
            };
        }

        public void RestoreFromSnapshot(TestSnapshotState snapshot)
        {
            state.Id = snapshot.Id;
            state.UpdatableField = snapshot.UpdatableField;
        }

        public TestSnapshotAggregateRootState State { get { return base.state; } }
    }
}
