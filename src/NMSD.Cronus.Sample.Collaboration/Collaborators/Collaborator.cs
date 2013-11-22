using System;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators
{
    public sealed class Collaborator : AggregateRoot<CollaboratorState>
    {
        Collaborator() { }

        public Collaborator(CollaboratorId collaboratorId, string email)
        {
            var evnt = new NewCollaboratorCreated(collaboratorId, email);
            state = new CollaboratorState();
            Apply(evnt);
        }

        public void Rename(string firstName, string lastName)
        {
            var evnt = new CollaboratorRenamed(state.Id, firstName, lastName);
            Apply(evnt);
        }
    }


}
