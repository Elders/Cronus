using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning;

[DataContract(Name = "9c0c789f-1605-460b-b179-cfdf2a697a1c")]
public sealed class NewProjectionVersionIsNowLive : ISystemEvent
{
    NewProjectionVersionIsNowLive() { }

    public NewProjectionVersionIsNowLive(ProjectionVersionManagerId id, ProjectionVersion projectionVersion)
    {
        Id = id;
        Timestamp = DateTimeOffset.UtcNow.ToFileTime();
        ProjectionVersion = projectionVersion;
    }

    [DataMember(Order = 1)]
    public ProjectionVersionManagerId Id { get; private set; }

    [DataMember(Order = 2)]
    public ProjectionVersion ProjectionVersion { get; private set; }

    [DataMember(Order = 3)]
    public long Timestamp { get; private set; }

    DateTimeOffset IMessage.Timestamp => Timestamp.ToDateTimeOffsetUtc();

    public override string ToString()
    {
        return $"New projection version `{ProjectionVersion}` is now live.";
    }
}
