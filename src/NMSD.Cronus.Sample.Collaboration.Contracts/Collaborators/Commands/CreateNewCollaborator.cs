using NMSD.Cronus.Core.Commanding;
namespace NMSD.Cronus.Sample.Collaboration.Collaborators.Commands
{
    public class CreateNewCollaborator : ICommand
    {
        CreateNewCollaborator() { }

        public CreateNewCollaborator(CollaboratorId id, string email)
        {
            Email = email;
            Id = id;
        }

        public CollaboratorId Id { get; private set; }

        public string Email { get; private set; }
    }
}