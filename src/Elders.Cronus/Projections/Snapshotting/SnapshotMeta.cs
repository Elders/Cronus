using System;

namespace Elders.Cronus.Projections.Snapshotting
{
    public class SnapshotMeta
    {
        SnapshotMeta() { }

        public SnapshotMeta(int revision, string projectionName)
        {
            if (revision < -1) throw new ArgumentException($"{revision} must be >= -1", nameof(revision));
            if (string.IsNullOrEmpty(projectionName)) throw new ArgumentNullException(nameof(projectionName));

            Revision = revision;
            ProjectionName = projectionName;
        }

        public int Revision { get; private set; }

        public string ProjectionName { get; private set; }

        public static SnapshotMeta From(ISnapshot snapshot) => new SnapshotMeta(snapshot.Revision, snapshot.ProjectionName);
    }
}
