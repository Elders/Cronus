using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "ed0d9b4e-3ac5-4cd4-9598-7bf5687b037a")]
    public class ProjectionCommit
    {
        ProjectionCommit() { }

        public ProjectionCommit(IBlobId projectionId, ProjectionVersion version, IEvent @event, int snapshotMarker, EventOrigin eventOrigin, DateTime timeStamp)
        {
            ProjectionId = projectionId;
            ContractId = version.ProjectionName;
            Event = @event;
            SnapshotMarker = snapshotMarker;
            EventOrigin = eventOrigin;
            TimeStamp = timeStamp;
            Version = version;
        }

        [DataMember(Order = 1)]
        public IBlobId ProjectionId { get; private set; }

        [DataMember(Order = 2)]
        public Type ProjectionType { get; set; }

        [DataMember(Order = 3)]
        public IEvent Event { get; private set; }

        [DataMember(Order = 4)]
        public int SnapshotMarker { get; private set; }

        [DataMember(Order = 5)]
        public EventOrigin EventOrigin { get; private set; }

        [DataMember(Order = 6)]
        public DateTime TimeStamp { get; private set; }

        [DataMember(Order = 7)]
        public string ContractId { get; private set; }

        [DataMember(Order = 8)]
        public ProjectionVersion Version { get; private set; }
    }
}
