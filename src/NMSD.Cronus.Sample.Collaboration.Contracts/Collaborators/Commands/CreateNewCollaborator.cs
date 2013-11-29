using System.Runtime.Serialization;
using NMSD.Cronus.Core.Commanding;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators.Commands
{
    [DataContract(Name = "279e6378-af27-47e8-a34f-12ca3d371714", Namespace = "NMSD.Cronus.Sample.Collaboration")]
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
    }
}