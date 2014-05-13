using System;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Events;

namespace Elders.Cronus.Sample.Collaboration.Users.Ports
{
    public class UserPort : IPort,
        IMessageHandler<AccountRegistered>
    {
        public IPublisher<ICommand> CommandPublisher { get; set; }

        public void Handle(AccountRegistered message)
        {
            UserId userId = new UserId(Guid.NewGuid());
            var email = message.Email;
            CommandPublisher.Publish(new CreateUser(userId, email));
        }
    }
}
