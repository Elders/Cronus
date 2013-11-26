using System.Threading.Tasks;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators
{
    public class CollaboratorAppService : AggregateRootApplicationService<Collaborator>, ICommandHandler,
        ICommandHandler<CreateNewCollaborator>,
        ICommandHandler<RenameCollaborator>
    {
        public void Handle(RenameCollaborator command)
        {
            UpdateAggregate(command.Id, ar => ar.Rename(command.FirstName, command.LastName));
        }

        public void Handle(CreateNewCollaborator command)
        {
            CreateAggregate(new Collaborator(command.Id, command.Email));
        }
    }
}