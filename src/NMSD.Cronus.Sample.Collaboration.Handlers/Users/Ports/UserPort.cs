using System;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Sample.Collaboration.Users.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Events;

namespace NMSD.Cronus.Sample.Collaboration.Users.Ports
{
    public class UserPort : IPort,
        IMessageHandler<AccountRegistered>
    {
        public IPublisher CommandPublisher { get; set; }

        public void Handle(AccountRegistered message)
        {
            UserId userId = new UserId(Guid.NewGuid());
            var email = message.Email;
            CommandPublisher.Publish(new CreateUser(userId, email));
        }
    }
}
