using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Sample.IdentityAndAccess.Users;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;

namespace NMSD.Cronus.Sample.IdentityAndAccess
{
    public sealed class User : AggregateRoot<UserState>
    {
        User() { }

        public User(UserId userId, string email)
        {
            var evnt = new NewUserRegistered(userId, email);
            state = new UserState();
            Apply(evnt);
        }

    }
}
