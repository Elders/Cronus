using Elders.Cronus.Testing;
using Machine.Specifications;
using System;

namespace Elders.Cronus.Snapshots
{
    [Subject(nameof(SnapshotManager))]
    public class When_creating_a_snapshot
    {
        static SnapshotManagerId snapshotId;
        static string contract;
        static SnapshotManager snapshot;
        static Exception exception;
        static RevisionStatus completed;
        static RevisionStatus running;
        static RevisionStatus canceled;
        static RevisionStatus failed;

        static int revision;

        Establish context = () =>
        {
            snapshotId = SnapshotCreator.SnapshotManagerId;
            contract = SnapshotCreator.Contract;
            snapshot = new SnapshotManager(snapshotId);

            completed = SnapshotCreator.CompletedStatus;
            running = SnapshotCreator.RunningStatus;
            canceled = SnapshotCreator.CanceledStatus;
            failed = SnapshotCreator.FailedStatus;
            revision = 1;

            snapshot = Aggregate<SnapshotManager>
             .FromHistory(stream => stream
                 .AddEvent(new SnapshotRequested(snapshotId, running, contract, DateTimeOffset.UtcNow)));
        };

        class When_creating_a_snapshot_without_id
        {
            Because of = () => exception = Catch.Exception(() => new SnapshotManager(null));
            It should_fail_with = () => exception.ShouldBeOfExactType<ArgumentNullException>();
        }

        class When_completing_a_snapshot
        {
            Because of = () => snapshot.Complete(revision);

            It should = () => snapshot.IsEventPublished<SnapshotCompleted>().ShouldBeTrue();
            It should_contain_revision = () => snapshot.RootState().GetRevisionStatus(revision).Status.ShouldBeTheSameAs(completed.Status);
        }

        class When_canceling_a_snapshot
        {
            Because of = () => snapshot.Cancel(revision);

            It should = () => snapshot.IsEventPublished<SnapshotCanceled>().ShouldBeTrue();
            It should_contain_revision = () => snapshot.RootState().GetRevisionStatus(revision).Status.ShouldBeTheSameAs(canceled.Status);
        }

        class When_snapshot_failed
        {
            Because of = () => snapshot.Fail(revision);

            It should = () => snapshot.IsEventPublished<SnapshotFailed>().ShouldBeTrue();
            It should_contain_revision = () => snapshot.RootState().GetRevisionStatus(revision).Status.ShouldBeTheSameAs(failed.Status);
        }
    }
}
