using NMSD.Cronus.Core.Commanding;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators.Commands
{
    public class RenameCollaborator : ICommand
    {
        RenameCollaborator() { }

        public RenameCollaborator(CollaboratorId id, string firstName, string lastName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
        }

        public CollaboratorId Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}