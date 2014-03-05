using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Events;

namespace NMSD.Cronus.Sample.IdentityAndAccess.Accounts
{
    public sealed class Account : AggregateRoot<AccountState>
    {
        Account() { }

        public Account(AccountId userId, string email)
        {
            var evnt = new AccountRegistered(userId, email);
            state = new AccountState();
            Apply(evnt);
        }

        public void ChangeEmail(string oldEmail, string newEmail)
        {
            //  Checks
            var @event = new AccountEmailChanged(state.Id, oldEmail, newEmail);
            Apply(@event);
        }

    }
}
