using System.Runtime.Serialization;
using System.Threading.Tasks;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators
{

    public class CollaboratorAppService : AggregateRootApplicationService<Collaborator>, IMessageHandler,
        IMessageHandler<CreateNewCollaborator>,
        IMessageHandler<RenameCollaborator>,
        IMessageHandler<TestRenameCollaborator>
    {
        public void Handle(RenameCollaborator command)
        {
            UpdateAggregate(command.Id, ar => ar.Rename(command.FirstName, command.LastName));
        }

        public void Handle(CreateNewCollaborator command)
        {
            CreateAggregate(new Collaborator(command.Id, command.Email));
        }

        public void Handle(TestRenameCollaborator command)
        {
            throw new System.NotImplementedException();
        }
    }
}