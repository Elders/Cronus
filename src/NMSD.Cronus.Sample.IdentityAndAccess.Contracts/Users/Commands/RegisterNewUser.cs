using System.Runtime.Serialization;
using NMSD.Cronus.Commanding;

namespace NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands
{
    [DataContract(Name = "2826a407-4eb1-4e28-a296-40be4ec9f23f")]
    public class RegisterNewUser : ICommand
    {
        RegisterNewUser() { }

        public RegisterNewUser(UserId id, string email)
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
            return this.ToString("Register a new user with '{0}' email. {1}", Email, Id);
        }
    }
}