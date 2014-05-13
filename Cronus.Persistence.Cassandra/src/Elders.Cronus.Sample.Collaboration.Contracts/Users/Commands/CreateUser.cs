using System.Runtime.Serialization;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Sample.Collaboration.Users.Commands
{
    [DataContract(Name = "279e6378-af27-47e8-a34f-12ca3d371714")]
    public class CreateUser : Command
    {
        CreateUser() { }

        public CreateUser(UserId id, string email)
            : base(id)
        {
            Email = email;
            Id = id;
        }

        [DataMember(Order = 1)]
        public UserId Id { get; private set; }

        [DataMember(Order = 2)]
        public string Email { get; private set; }

        public override string ToString()
        {
            return this.ToString("Create a new user with '{0}' email. {1}", Email, Id);
        }
    }
}