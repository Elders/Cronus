using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Namespace = "cronus", Name = "1d249c16-555c-4463-92dc-54218d9a3245")]
    public sealed class ProjectionVersionRequestTimedout : ISystemEvent
    {
        ProjectionVersionRequestTimedout() { }

        public ProjectionVersionRequestTimedout(ProjectionVersionManagerId id, ProjectionVersion version, VersionRequestTimebox timebox)
        {
            Id = id;
            Version = version;
            RequestTimestamp = DateTimeOffset.UtcNow.ToFileTime();
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

        public DateTimeOffset Timestamp => RequestTimestamp.ToDateTimeOffsetUtc();

        public override string ToString()
        {
            return $"Timeout projection version request for {Version} with timebox {Timebox}";
        }
    }

    [DataContract(Namespace = "cronus", Name = "20ca3662-e377-43d4-b0ce-f6419c4c185a")]
    public sealed class ProjectionVersionRequestPaused : ISystemEvent
    {
        ProjectionVersionRequestPaused() { }

        public ProjectionVersionRequestPaused(ProjectionVersionManagerId id, ProjectionVersion version, DateTimeOffset timestamp)
        {
            Id = id;
            Version = version;
            Timestamp = timestamp;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion Version { get; private set; }

        [DataMember(Order = 3)]
        public DateTimeOffset Timestamp { get; private set; }

        public override string ToString()
        {
            return $"Projection version request paused for {Version}`";
        }
    }
}
