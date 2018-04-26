using System.Runtime.Serialization;
using System;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "e9731995-bfba-4de8-bd01-f5d8c06dba6d")]
    public class ProjectionVersionRequestCanceled : IEvent
    {
        ProjectionVersionRequestCanceled() { }

        public ProjectionVersionRequestCanceled(ProjectionVersionManagerId id, ProjectionVersion projectionVersion, string reason)
        {
            Id = id;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
            ProjectionVersion = projectionVersion;
            Reason = reason;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion ProjectionVersion { get; private set; }

        [DataMember(Order = 3)]
        public long Timestamp { get; private set; }

        [DataMember(Order = 4)]
        public string Reason { get; private set; }
    }
}
