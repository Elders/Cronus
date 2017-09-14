using System;
using Elders.Cronus.DomainModeling;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "ed0d9b4e-3ac5-4cd4-9598-7bf5687b037a")]
    public class ProjectionCommit
    {
        ProjectionCommit() { }

        public ProjectionCommit(IBlobId projectionId, string contractId, IEvent @event, int snapshotMarker, EventOrigin eventOrigin, DateTime timeStamp)
        {
            ProjectionId = projectionId;
            ContractId = contractId;
            Event = @event;
            SnapshotMarker = snapshotMarker;
            EventOrigin = eventOrigin;
            TimeStamp = timeStamp;
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
        public EventOrigin EventOrigin { get; set; }

        [DataMember(Order = 6)]
        public DateTime TimeStamp { get; set; }

        [DataMember(Order = 7)]
        public string ContractId { get; set; }
    }
}
