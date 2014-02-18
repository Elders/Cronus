using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators
{

    public class CollaboratorAppService : AggregateRootApplicationService<Collaborator>, IMessageHandler,
        IMessageHandler<CreateNewCollaborator>,
        IMessageHandler<RenameCollaborator>
    {
        public void Handle(RenameCollaborator command)
        {
            Repository.Update<Collaborator>(command.Id, user => user.Rename(command.FirstName, command.LastName));
        }

        public void Handle(CreateNewCollaborator command)
        {
            Repository.Save(new Collaborator(command.Id, command.Email));
        }
    }
}