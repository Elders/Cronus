using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;

namespace NMSD.Cronus.Sample.IdentityAndAccess.Users
{
    public class UserAppService : AggregateRootApplicationService<User>, IMessageHandler,
        IMessageHandler<RegisterNewUser>
    {

        public void Handle(RegisterNewUser message)
        {
            CreateAggregate(new User(message.UserId, message.Email));
        }
    }
}
