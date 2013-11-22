using System;
using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators
{
    public class CollaboratorId : AggregateRootId
    {
        public CollaboratorId(Guid id) : base(id) { }
    }
}