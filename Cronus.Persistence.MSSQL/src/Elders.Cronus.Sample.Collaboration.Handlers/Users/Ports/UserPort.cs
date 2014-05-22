using System;
using System.Linq;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.Collaboration.Users.DTOs;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Events;
using NHibernate;
using NHibernate.Linq;

namespace Elders.Cronus.Sample.Collaboration.Users.Ports
{
    public class UserPort : IPort, IHaveNhibernateSession,
        IMessageHandler<AccountRegistered>,
        IMessageHandler<AccountEmailChanged>
    {
        public IPublisher<ICommand> CommandPublisher { get; set; }

        public ISession Session { get; set; }

        public void Handle(AccountEmailChanged message)
        {
            var user = Session.Query<User>().Where(x => x.Email == message.OldEmail).Single();
            CommandPublisher.Publish(new ChangeEmail(new UserId(user.Id), message.NewEmail));
        }

        public void Handle(AccountRegistered message)
        {
            UserId userId = new UserId(Guid.NewGuid());
            var email = message.Email;
            CommandPublisher.Publish(new CreateUser(userId, email));
        }
    }
}
