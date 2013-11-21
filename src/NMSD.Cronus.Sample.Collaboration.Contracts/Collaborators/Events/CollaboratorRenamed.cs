using System;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators.Events
{
    public class CollaboratorRenamed : ICollaboratorEvent
    {
        public CollaboratorRenamed(CollaboratorId collaboratorId, string firstName, string lastName)
        {
            CollaboratorId = collaboratorId;
            FirstName = firstName;
            LastName = lastName;
        }

        public CollaboratorId CollaboratorId { get; private set; }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

    }
}