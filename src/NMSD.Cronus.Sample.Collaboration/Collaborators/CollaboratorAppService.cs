using NMSD.Cronus.Core.Cqrs;
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
            var user = EventStore.Load<Collaborator>(command.Id);
            user.Rename(command.FirstName, command.LastName);
            EventStore.Save(user);
        }

        public void Handle(CreateNewCollaborator command)
        {
            EventStore.Save(new Collaborator(command.Id, command.Email));
        }
    }
}