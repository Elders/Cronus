using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;

namespace NMSD.Cronus.Sample.IdentityAndAccess.Users
{
    public class UserAppService : AggregateRootApplicationService<User>, IMessageHandler,
        IMessageHandler<RegisterNewUser>,
        IMessageHandler<ChangeUserEmail>
    {

        public void Handle(RegisterNewUser message)
        {
            EventStore.Save(new User(message.Id, message.Email));
        }

        public void Handle(ChangeUserEmail message)
        {
            var user = EventStore.Load<User>(message.Id);
            user.ChangeEmail(message.OldEmail, message.NewEmail);
            EventStore.Save(user);
        }
    }
}
