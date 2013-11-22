using System;
using System.Runtime.Serialization;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators.Events
{
    [DataContract(Name = "8caa4c0c-4a34-4267-a8ef-b1fbe11d03c3")]
    public class NewCollaboratorCreated : ICollaboratorEvent
    {
        public NewCollaboratorCreated(CollaboratorId collaboratorId, string email)
        {
            CollaboratorId = collaboratorId;
            Email = email;
        }

        [DataMember(Order = 1)]
        public CollaboratorId CollaboratorId { get; private set; }

        [DataMember(Order = 2)]
        public string Email { get; private set; }

        public override string ToString()
        {
            return String.Format("New collaborator created with email '{0}'. {1}", Email, CollaboratorId);
        }
    }
}