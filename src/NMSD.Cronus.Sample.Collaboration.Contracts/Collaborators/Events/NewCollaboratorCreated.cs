using System;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators.Events
{
    public class NewCollaboratorCreated : ICollaboratorEvent
    {
        public NewCollaboratorCreated(CollaboratorId collaboratorId, string email)
        {
            CollaboratorId = collaboratorId;
            Email = email;
        }

        public CollaboratorId CollaboratorId { get; private set; }

        public string Email { get; private set; }

        public override string ToString()
        {
            return String.Format("New collaborator created with email '{0}'. {1}", Email, CollaboratorId);
        }
    }
}