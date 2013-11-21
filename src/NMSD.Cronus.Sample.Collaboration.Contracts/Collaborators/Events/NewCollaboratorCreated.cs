using System;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators.Events
{
    public class NewCollaboratorCreated : ICollaboratorEvent
    {
        public NewCollaboratorCreated(CollaboratorId collaboratorId, string email)
        {
            CollaboratorId = collaboratorId;
        }

        public CollaboratorId CollaboratorId { get; private set; }

        public string Email { get; private set; }
    }
}