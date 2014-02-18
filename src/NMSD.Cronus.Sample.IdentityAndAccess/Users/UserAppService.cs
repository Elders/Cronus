using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;

namespace NMSD.Cronus.Sample.IdentityAndAccess.Users
{
    public class UserAppService : AggregateRootApplicationService<User>, IMessageHandler,
        IMessageHandler<RegisterNewUser>,
        IMessageHandler<ChangeUserEmail>
    {

        public void Handle(RegisterNewUser message)
        {
            Repository.Save(new User(message.Id, message.Email));
        }

        public void Handle(ChangeUserEmail message)
        {
            Repository.Update<User>(message.Id, user => user.ChangeEmail(message.OldEmail, message.NewEmail));
        }
    }
}
