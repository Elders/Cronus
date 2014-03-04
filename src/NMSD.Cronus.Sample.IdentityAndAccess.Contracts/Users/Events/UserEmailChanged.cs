using System.Runtime.Serialization;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Sample.IdentityAndAccess.Users.Events
{
    [DataContract(Name = "fa54bc57-cfba-49b6-b08f-99457624de5d")]
    public class UserEmailChanged : IEvent
    {
        UserEmailChanged() { }

        public UserEmailChanged(UserId id, string oldEmail, string newEmail)
        {
            NewEmail = newEmail;
            OldEmail = oldEmail;
            Id = id;
        }

        [DataMember(Order = 1)]
        public UserId Id { get; private set; }

        [DataMember(Order = 2)]
        public string OldEmail { get; private set; }

        [DataMember(Order = 3)]
        public string NewEmail { get; private set; }

        public override string ToString()
        {
            return this.ToString("User email '{0}' was changed to '{1}'. {2}", OldEmail, NewEmail, Id);
        }
    }
}
