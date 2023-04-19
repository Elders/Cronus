using Elders.Cronus.Tests.TestModel;
using System;

namespace Elders.Cronus.Snapshots
{
    public static class SnapshotCreator
    {
        private static SnapshotManagerId snapshotManagerId = new SnapshotManagerId(new TestAggregateId(), "cronustest");

        private static string contract = "103f25df-3307-42a0-a643-a344c6e9bf81";

        private static RevisionStatus completed = new RevisionStatus(1, SnapshotRevisionStatus.Completed, DateTimeOffset.UtcNow);
        private static RevisionStatus running = new RevisionStatus(1, SnapshotRevisionStatus.Running, DateTimeOffset.UtcNow);
        private static RevisionStatus canceled = new RevisionStatus(1, SnapshotRevisionStatus.Canceled, DateTimeOffset.UtcNow);
        private static RevisionStatus fail = new RevisionStatus(1, SnapshotRevisionStatus.Failed, DateTimeOffset.UtcNow);

        internal static SnapshotManagerId SnapshotManagerId => snapshotManagerId;

        internal static RevisionStatus CompletedStatus => completed;

        internal static RevisionStatus RunningStatus => running;

        internal static RevisionStatus CanceledStatus => canceled;

        internal static RevisionStatus FailedStatus => fail;

        internal static string Contract => contract;
    }
}
