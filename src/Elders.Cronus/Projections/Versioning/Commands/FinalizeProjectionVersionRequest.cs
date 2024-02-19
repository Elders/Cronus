using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "c0ea19c2-3630-41de-a3e7-54811a5b5a30")]
    public sealed class FinalizeProjectionVersionRequest : ISystemCommand
    {
        FinalizeProjectionVersionRequest()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        public FinalizeProjectionVersionRequest(ProjectionVersionManagerId id, ProjectionVersion version) : this()
        {
            Id = id;
            Version = version;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion Version { get; private set; }

        [DataMember(Order = 3)]
        public DateTimeOffset Timestamp { get; private set; }

        public override string ToString()
        {
            return $"Finalize projection version {Version}. {nameof(ProjectionVersionManagerId)}: {Id}";
        }
    }
}
