using System.Runtime.Serialization;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Sample.Collaboration.Users.Events
{
    [DataContract(Name = "8caa4c0c-4a34-4267-a8ef-b1fbe11d03c3")]
    public class UserCreated : IEvent
    {
        UserCreated() { }

        public UserCreated(UserId id, string email)
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
            return this.ToString("New user created with email '{0}'. {1}", Email, Id);
        }
    }
}