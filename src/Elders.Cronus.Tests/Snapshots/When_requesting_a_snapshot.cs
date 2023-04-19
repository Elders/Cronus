using Elders.Cronus.Snapshots.Options;
using Elders.Cronus.Snapshots.Strategy;
using Elders.Cronus.Testing;
using Machine.Specifications;
using System;

namespace Elders.Cronus.Snapshots
{
    [Subject(nameof(SnapshotManager))]
    public class When_requesting_a_snapshot
    {
        static SnapshotManagerId snapshotId;
        static string contract;
        static SnapshotManager snapshot;
        static SnapshotManager alreadyRequestedSnapshot;

        static RevisionStatus completed;
        static RevisionStatus running;
        static RevisionStatus runningRevisionForALongTime;
        static RevisionStatus canceled;
        static RevisionStatus failed;
        static ISnapshotStrategy<AggregateSnapshotStrategyContext> snapshotStrategy;
        static SnapshotManagerOptions options;

        static int revision;

        Establish context = () =>
        {
            contract = SnapshotCreator.Contract;
            snapshotId = SnapshotCreator.SnapshotManagerId;
            snapshot = new SnapshotManager(snapshotId);

            alreadyRequestedSnapshot = new SnapshotManager(snapshotId);

            completed = SnapshotCreator.CompletedStatus;
            running = SnapshotCreator.RunningStatus;
            runningRevisionForALongTime = new RevisionStatus(1, SnapshotRevisionStatus.Running, DateTimeOffset.UtcNow.AddMinutes(-10));
            canceled = SnapshotCreator.CanceledStatus;
            failed = SnapshotCreator.FailedStatus;
            revision = 2;

            snapshotStrategy = new CreateSnapshotNowStrategy();
            options = new SnapshotManagerOptions();

            alreadyRequestedSnapshot = Aggregate<SnapshotManager>
             .FromHistory(stream => stream
                 .AddEvent(new SnapshotRequested(snapshotId, runningRevisionForALongTime, contract, DateTimeOffset.UtcNow)));
        };

        class And_it_is_sucessuful
        {
            Because of = async () => await snapshot.RequestSnapshotAsync(snapshotId, revision, contract, revision, new TimeSpan(0), snapshotStrategy, options);

            It should_request_snapshot = () => snapshot.IsEventPublished<SnapshotRequested>().ShouldBeTrue();
            It should_be_running = () => snapshot.RootState().GetRevisionStatus(revision).Status.ShouldBeTheSameAs(running.Status);
        }

        class And_last_revision_is_still_running_after_snapshot_timeout
        {
            Because of = async () => await alreadyRequestedSnapshot.RequestSnapshotAsync(snapshotId, revision, contract, revision, new TimeSpan(0), snapshotStrategy, options);

            It should_apply_event_to_cancel_request_to_create_prev_snapshot = () => alreadyRequestedSnapshot.IsEventPublished<SnapshotCanceled>().ShouldBeTrue();
            It should_apply_event_to_request_a_new_snapshot = () => alreadyRequestedSnapshot.IsEventPublished<SnapshotRequested>().ShouldBeTrue();
            It should_be_canceled = () => alreadyRequestedSnapshot.RootState().GetRevisionStatus(revision).Status.ShouldBeTheSameAs(running.Status);
        }
    }
}
