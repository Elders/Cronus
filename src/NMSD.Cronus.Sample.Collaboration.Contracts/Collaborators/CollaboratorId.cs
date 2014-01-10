using System;
using System.Runtime.Serialization;
using NMSD.Cronus.DomainModelling;


namespace NMSD.Cronus.Sample.Collaboration.Collaborators
{
    [DataContract(Name = "03ba718c-58b8-46e9-978f-c4675e584929")]
    public class CollaboratorId : AggregateRootId
    {
        CollaboratorId() { }
        public CollaboratorId(Guid id) : base(id) { }
    }
}