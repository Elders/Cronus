using System.Runtime.Serialization;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands
{
    [DataContract(Name = "645b65a5-9381-44c5-9d11-7c67c1e3ce34")]
    public class ChangeAccountEmail : Command
    {
        ChangeAccountEmail() { }

        public ChangeAccountEmail(AccountId id, string oldEmail, string newEmail)
            : base(id)
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
            return this.ToString("Change old account email '{0}' with '{1}'. {2}", OldEmail, NewEmail, Id);
        }
    }
}
