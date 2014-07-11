using System.Runtime.Serialization;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Sample.Collaboration.Users.Commands
{
    [DataContract(Name = "aeaae173-d790-443d-92b2-caa06d55f1a2")]
    public class RenameUser : ICommand
    {
        RenameUser() { }

        public RenameUser(UserId id, string firstName, string lastName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
        }

        [DataMember(Order = 1)]
        public UserId Id { get; set; }

        [DataMember(Order = 2)]
        public string FirstName { get; set; }

        [DataMember(Order = 3)]
        public string LastName { get; set; }
    }

    [DataContract(Name = "67d803e9-57de-4207-8ed6-5d679b7dc552")]
    public class ChangeEmail : ICommand
    {
        ChangeEmail() { }

        public ChangeEmail(UserId id, string newEmail)
        {
            Id = id;
            NewEmail = newEmail;
        }

        [DataMember(Order = 1)]
        public UserId Id { get; set; }

        [DataMember(Order = 2)]
        public string NewEmail { get; set; }
    }
}