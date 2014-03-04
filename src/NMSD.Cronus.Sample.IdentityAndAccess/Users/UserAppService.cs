using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;

namespace NMSD.Cronus.Sample.IdentityAndAccess.Users
{
    public class UserAppService : AggregateRootApplicationService<User>,
        IMessageHandler<RegisterNewUser>,
        IMessageHandler<ChangeUserEmail>
    {

        public void Handle(RegisterNewUser message)
        {
            Repository.Save(new User(message.Id, message.Email), message);
        }

        public void Handle(ChangeUserEmail message)
        {
            Repository.Update<User>(message.Id, message, user => user.ChangeEmail(message.OldEmail, message.NewEmail));
        }
    }
}
