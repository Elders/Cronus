using System.Runtime.Serialization;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators.Events
{
    [DataContract(Name = "64089974-6371-4112-84dc-4326ab3ec52e")]
    public class CollaboratorRenamed : IEvent
    {
        CollaboratorRenamed() { }

        public CollaboratorRenamed(CollaboratorId collaboratorId, string firstName, string lastName)
        {
            CollaboratorId = collaboratorId;
            FirstName = firstName;
            LastName = lastName;
        }

        [DataMember(Order = 1)]
        public CollaboratorId CollaboratorId { get; private set; }

        [DataMember(Order = 2)]
        public string FirstName { get; private set; }

        [DataMember(Order = 3)]
        public string LastName { get; private set; }
    }
}