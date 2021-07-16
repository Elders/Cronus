using System.Runtime.Serialization;
using System;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "b3c7fdc0-ee95-4aeb-b7c3-a51e08fbd289")]
    public class ProjectionFinishedRebuilding : ISystemEvent
    {
        ProjectionFinishedRebuilding() { }

        public ProjectionFinishedRebuilding(ProjectionVersionManagerId id, ProjectionVersion projectionVersion)
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
            return $"Projection version `{ProjectionVersion}` has finished rebuilding.";
        }
    }
}
