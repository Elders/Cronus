using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "e9731995-bfba-4de8-bd01-f5d8c06dba6d")]
    public sealed class ProjectionVersionRequestCanceled : ISystemEvent
    {
        ProjectionVersionRequestCanceled() { }

        public ProjectionVersionRequestCanceled(ProjectionVersionManagerId id, ProjectionVersion version, string reason)
        {
            Id = id;
            Timestamp = DateTimeOffset.UtcNow.ToFileTime();
            Version = version;
            Reason = reason;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion Version { get; private set; }

        [DataMember(Order = 3)]
        public long Timestamp { get; private set; }

        [DataMember(Order = 4)]
        public string Reason { get; private set; }

        DateTimeOffset IMessage.Timestamp => Timestamp.ToDateTimeOffsetUtc();

        public override string ToString()
        {
            return $"Projection version request has been canceled for `{Version}`. {Environment.NewLine}{nameof(ProjectionVersionManagerId)}: `{Id}`.{Environment.NewLine}Timestamp: `{Timestamp}`.{Environment.NewLine}Reason: `{Reason}`.";
        }
    }
}
