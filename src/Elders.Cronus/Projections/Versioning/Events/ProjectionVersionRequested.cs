using System;
using System.Runtime.Serialization;
using Elders.Cronus.EventStore.Players;

namespace Elders.Cronus.Projections.Versioning;

[DataContract(Name = "5788a757-5dd6-4680-8f24-add1dfa7539b")]
public sealed class ProjectionVersionRequested : ISystemEvent
{
    ProjectionVersionRequested() { }

    public ProjectionVersionRequested(ProjectionVersionManagerId id, ProjectionVersion projectionVersion, ReplayEventsOptions replayEventsOptions, VersionRequestTimebox timebox)
    {
        Id = id;
        Version = projectionVersion;
        RequestTimestamp = DateTime.UtcNow.ToFileTimeUtc();
        ReplayEventsOptions = replayEventsOptions;
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

    [DataMember(Order = 5)]
    public ReplayEventsOptions ReplayEventsOptions { get; set; }

    public DateTimeOffset Timestamp => RequestTimestamp.ToDateTimeOffsetUtc();

    public override string ToString()
    {
        return $"Projection version `{Version}` was requested with timebox:{Timebox}";
    }
}
