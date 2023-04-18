using Elders.Cronus.Testing;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;
using System;

namespace Elders.Cronus.Snapshots
{
    public class When_loading_aggreagate_from_snapshot
    {
        static TestSnapshotAggregateId arId;
        static TestSnapshotAggregateRoot ar;
        static TestSnapshotState snapshotState;

        Establish context = () =>
        {
            arId = new TestSnapshotAggregateId();

            ar = Aggregate<TestSnapshotAggregateRoot>
            .FromHistory(stream => stream
                .AddEvent(new TestSnapshotCreateEvent(arId))
                .AddEvent(new TestSnapshotUpdateEvent(arId, Guid.NewGuid().ToString())));
        };

        class And_it_the_same_as_the_ar_state
        {
            Because of = () => snapshotState = ar.CreateSnapshot();
            It should_be_true = () => ar.State.UpdatableField.ShouldBeTheSameAs(snapshotState.UpdatableField);
        }
    }
}
