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
        static int counter = 0;
        public void Handle(AccountRegistered message)
        {
            //counter++;
            UserId userId = new UserId(Guid.NewGuid());
            var email = message.Email;
            //if (counter % 500 == 0)
            //    email = "cronus_2_@Elders.com";

            CommandPublisher.Publish(new CreateUser(userId, email));
        }
    }
}
