using NMSD.Cronus.Core.DomainModelling;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators
{

    public class CollaboratorAppService : AggregateRootApplicationService<Collaborator>, IMessageHandler,
        IMessageHandler<CreateNewCollaborator>,
        IMessageHandler<RenameCollaborator>
    {
        public void Handle(RenameCollaborator command)
        {
            var user = Repository.Load<Collaborator>(command.Id);
            user.Rename(command.FirstName, command.LastName);
            Repository.Save(user);
        }

        public void Handle(CreateNewCollaborator command)
        {
            Repository.Save(new Collaborator(command.Id, command.Email));
        }
    }
}