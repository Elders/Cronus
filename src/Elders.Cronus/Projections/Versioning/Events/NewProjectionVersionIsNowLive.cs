using System.Runtime.Serialization;
using System;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "9c0c789f-1605-460b-b179-cfdf2a697a1c")]
    public class NewProjectionVersionIsNowLive : IEvent
    {
        NewProjectionVersionIsNowLive() { }

        public NewProjectionVersionIsNowLive(ProjectionVersionManagerId id, ProjectionVersion projectionVersion, string tenant)
        {
            Id = id;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
            ProjectionVersion = projectionVersion;
            Tenant = tenant;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion ProjectionVersion { get; private set; }

        [DataMember(Order = 3)]
        public long Timestamp { get; private set; }

        [DataMember(Order = 4)]
        public string Tenant { get; private set; }

        public override string ToString()
        {
            return $"New projection version `{ProjectionVersion}` is now live.";
        }
    }
}
