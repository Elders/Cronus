using System.Runtime.Serialization;

namespace Elders.Cronus.Projections;

[DataContract(Name = "db13e442-a6d2-4247-9e5f-86931907f00b")]
public sealed class ProjectionCommit // nah, create a new one 
{
    ProjectionCommit() { }

    public ProjectionCommit(IBlobId projectionId, ProjectionVersion version, IEvent @event, long partition)
    {
        ProjectionId = projectionId;
        Version = version;
        Event = @event;
        Partition = partition;
    }

    [DataMember(Order = 1)]
    public IBlobId ProjectionId { get; private set; }

    [DataMember(Order = 2)]
    public ProjectionVersion Version { get; private set; }

    [DataMember(Order = 3)]
    public IEvent Event { get; private set; }

    [DataMember(Order = 4)]
    public long Partition { get; private set; } // maybe not the right place here
}
