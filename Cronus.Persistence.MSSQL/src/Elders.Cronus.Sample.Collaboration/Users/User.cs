using System;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Sample.Collaboration.Users.Events;

namespace Elders.Cronus.Sample.Collaboration.Users
{
    public sealed class User : AggregateRoot<UserState>
    {
        User() { }

        public User(UserId collaboratorId, string email)
        {
            var evnt = new UserCreated(collaboratorId, email);
            state = new UserState();
            Apply(evnt);
        }

        public void Rename(string firstName, string lastName)
        {
            var evnt = new UserRenamed(state.Id, firstName, lastName);
            Apply(evnt);
        }

        public void ChangeEmail(string newEmail)
        {
            var evnt = new EmailChanged(state.Id, newEmail);
            Apply(evnt);
        }
    }
}
