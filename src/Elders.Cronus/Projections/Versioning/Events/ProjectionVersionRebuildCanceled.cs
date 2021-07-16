using System.Runtime.Serialization;
using System;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "d5952688-50c9-4d71-80b7-3a24baa4a83c")]
    public class ProjectionVersionRebuildCanceled : ISystemEvent
    {
        ProjectionVersionRebuildCanceled() { }

        public ProjectionVersionRebuildCanceled(ProjectionVersionManagerId id, ProjectionVersion projectionVersion)
        {
            Id = id;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
            ProjectionVersion = projectionVersion;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion ProjectionVersion { get; private set; }

        [DataMember(Order = 3)]
        public long Timestamp { get; private set; }

        public override string ToString()
        {
            return $"Live projection version `{ProjectionVersion}` failed rebuilding.";
        }
    }
}
