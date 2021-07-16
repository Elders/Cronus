using System.Runtime.Serialization;
using System;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "fdbf1217-8280-493d-80dd-e8eb97fd92a3")]
    public class ProjectionVersionRequestedForRebuild : ISystemEvent
    {
        ProjectionVersionRequestedForRebuild() { }

        public ProjectionVersionRequestedForRebuild(ProjectionVersionManagerId id, ProjectionVersion projectionVersion, VersionRequestTimebox timebox)
        {
            Id = id;
            Version = projectionVersion;
            RequestTimestamp = DateTime.UtcNow.ToFileTimeUtc();
            Timebox = timebox;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion Version { get; private set; }

        [DataMember(Order = 3)]
        public long RequestTimestamp { get; private set; }

        [DataMember(Order = 4)]
        public VersionRequestTimebox Timebox { get; private set; }

        public override string ToString()
        {
            return $"Projection version `{Version}` was requested for rebuild with timebox:{Timebox}";
        }
    }
}
