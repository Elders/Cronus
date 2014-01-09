using System;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;

namespace NMSD.Cronus.Sample.Collaboration.Ports
{
    public class CollaboratorPorts : IPort, IMessageHandler,
        IMessageHandler<NewUserRegistered>
    {
        public IPublisher<ICommand> CommandPublisher { get; set; }

        public void Handle(NewUserRegistered message)
        {
            CommandPublisher.Publish(new CreateNewCollaborator(new CollaboratorId(Guid.NewGuid()), message.Email));
        }

    }
}
