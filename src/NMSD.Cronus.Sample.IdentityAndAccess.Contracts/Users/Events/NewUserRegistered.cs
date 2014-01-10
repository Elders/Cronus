using System.Runtime.Serialization;
using NMSD.Cronus.Eventing;

namespace NMSD.Cronus.Sample.IdentityAndAccess.Users.Events
{
    [DataContract(Name = "594c1ff2-07b1-42ac-b622-bc9d5045057a")]
    public class NewUserRegistered : IEvent
    {
        NewUserRegistered() { }

        public NewUserRegistered(UserId id, string email)
        {
            Id = id;
            Email = email;
        }

        [DataMember(Order = 1)]
        public UserId Id { get; private set; }

        [DataMember(Order = 2)]
        public string Email { get; private set; }

        public override string ToString()
        {
            return this.ToString("New user registered with email '{0}'. {1}", Email, Id);
        }
    }
}
