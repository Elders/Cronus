using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "c0ea19c2-3630-41de-a3e7-54811a5b5a30")]
    public class FinalizeProjectionVersionRequest : ICommand
    {
        FinalizeProjectionVersionRequest() { }

        public FinalizeProjectionVersionRequest(ProjectionVersionManagerId id, ProjectionVersion version)
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
