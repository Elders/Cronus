using System.Runtime.Serialization;
using System;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "5788a757-5dd6-4680-8f24-add1dfa7539b")]
    public class ProjectionVersionRequested : IEvent
    {
        ProjectionVersionRequested() { }

        public ProjectionVersionRequested(ProjectionVersionManagerId id, ProjectionVersion projectionVersion, VersionRequestTimebox timebox)
        {
            Id = id;
            ProjectionVersion = projectionVersion;
            RequestTimestamp = DateTime.UtcNow.ToFileTimeUtc();
            Timebox = timebox;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion ProjectionVersion { get; private set; }

        [DataMember(Order = 3)]
        public long RequestTimestamp { get; private set; }

        [DataMember(Order = 4)]
        public VersionRequestTimebox Timebox { get; private set; }
    }
}
