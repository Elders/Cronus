using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Namespace = "cronus", Name = "633bc496-7efb-4bfb-9eb2-cccceea09a21")]
    public sealed class CancelProjectionVersionRequest : ISystemCommand
    {
        CancelProjectionVersionRequest()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        public CancelProjectionVersionRequest(ProjectionVersionManagerId id, ProjectionVersion version, string reason) : this()
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

        [DataMember(Order = 4)]
        public DateTimeOffset Timestamp { get; private set; }

        public override string ToString()
        {
            return $"Cancel projection version {Version}. {nameof(ProjectionVersionManagerId)}: {Id}. Reason: {Reason}.";
        }
    }
}
