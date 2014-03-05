using System.Runtime.Serialization;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Events;

namespace NMSD.Cronus.Sample.IdentityAndAccess.Accounts
{
    [DataContract(Name = "9e97081e-d230-4351-b23a-6cbb65df4cbb")]
    public sealed class AccountState : AggregateRootState<AccountId>
    {
        public AccountState() { }

        [DataMember(Order = 1)]
        public override AccountId Id { get; set; }

        [DataMember(Order = 2)]
        public override int Version { get; set; }

        [DataMember(Order = 3)]
        public string Email { get; private set; }

        [DataMember(Order = 4)]
        public string Firstname { get; private set; }

        [DataMember(Order = 5)]
        public string LastName { get; private set; }

        public void When(AccountRegistered e)
        {
            Id = e.Id;
            Email = e.Email;
        }

        public void When(AccountEmailChanged e)
        {
            Email = e.NewEmail;
        }
    }
}
