using System.Runtime.Serialization;
using NMSD.Cronus.Commanding;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators.Commands
{
    [DataContract(Name = "279e6378-af27-47e8-a34f-12ca3d371714")]
    public class CreateNewCollaborator : ICommand
    {
        CreateNewCollaborator() { }

        public CreateNewCollaborator(CollaboratorId id, string email)
        {
            Email = email;
            Id = id;
        }

        [DataMember(Order = 1)]
        public CollaboratorId Id { get; private set; }

        [DataMember(Order = 2)]
        public string Email { get; private set; }

        public override string ToString()
        {
            return this.ToString("Create a new collaborator with '{0}' email. {1}", Email, Id);
        }
    }
}