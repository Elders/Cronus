using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators
{
    public class CollaboratorState : AggregateRootState<CollaboratorId>
    {
        public string Firstname { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }

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
