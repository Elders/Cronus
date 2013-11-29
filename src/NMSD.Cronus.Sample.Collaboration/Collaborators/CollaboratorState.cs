using System.Runtime.Serialization;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators
{
    [DataContract(Name = "c8978654-4380-44d2-8ebe-ae17a463dfb6", Namespace = "NMSD.Cronus.Sample.Collaboration")]
    public class CollaboratorState : AggregateRootState<CollaboratorId>
    {
        public CollaboratorState() { }

        [DataMember(Order = 1)]
        public string Email { get; private set; }

        [DataMember(Order = 2)]
        public string Firstname { get; private set; }

        [DataMember(Order = 3)]
        public string LastName { get; private set; }

        public void When(CollaboratorRenamed e)
        {
            Firstname = e.FirstName;
            LastName = e.LastName;
        }

        public void When(NewCollaboratorCreated e)
        {
            Id = e.CollaboratorId;
            Email = e.Email;
        }
    }
}
