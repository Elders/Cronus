using NMSD.Cronus.Core.DomainModelling;
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

        public void ChangeEmail(string oldEmail, string newEmail)
        {
            //  Checks
            var @event = new UserEmailChanged(state.Id, oldEmail, newEmail);
            Apply(@event);
        }

    }
}
