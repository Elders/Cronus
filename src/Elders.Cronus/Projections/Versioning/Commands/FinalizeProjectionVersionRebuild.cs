using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "9c7fd3a6-33ab-4f96-8bb8-337c741426da")]
    public class FinalizeProjectionVersionRebuild : ISystemCommand
    {
        FinalizeProjectionVersionRebuild() { }

        public FinalizeProjectionVersionRebuild(ProjectionVersionManagerId id, ProjectionVersion version)
        {
            Id = id;
            Version = version;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion Version { get; private set; }

        public override string ToString()
        {
            return $"Finalize projection rebuilding for version '{Version}'. {nameof(ProjectionVersionManagerId)}: `{Id}`";
        }
    }
}
