using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "8b40c32d-c62e-4ae7-bb65-dfe385b66509")]
    public class CancelVersionRebuild : ISystemCommand
    {
        CancelVersionRebuild() { }

        public CancelVersionRebuild(ProjectionVersionManagerId id, ProjectionVersion version, string reason)
        {
            Id = id;
            Version = version;
            Reason = reason;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion Version { get; private set; }

        [DataMember(Order = 3)]
        public string Reason { get; private set; }

        public override string ToString()
        {
            return $"Cancel projection rebuilding for version `{Version}`. {nameof(ProjectionVersionManagerId)}: `{Id}`. Reason: `{Reason}`.";
        }
    }
}
