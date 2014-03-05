using System.Runtime.Serialization;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Events
{
    [DataContract(Name = "fa54bc57-cfba-49b6-b08f-99457624de5d")]
    public class AccountEmailChanged : IEvent
    {
        AccountEmailChanged() { }

        public AccountEmailChanged(AccountId id, string oldEmail, string newEmail)
        {
            NewEmail = newEmail;
            OldEmail = oldEmail;
            Id = id;
        }

        [DataMember(Order = 1)]
        public AccountId Id { get; private set; }

        [DataMember(Order = 2)]
        public string OldEmail { get; private set; }

        [DataMember(Order = 3)]
        public string NewEmail { get; private set; }

        public override string ToString()
        {
            return this.ToString("Account email '{0}' was changed to '{1}'. {2}", OldEmail, NewEmail, Id);
        }
    }
}
