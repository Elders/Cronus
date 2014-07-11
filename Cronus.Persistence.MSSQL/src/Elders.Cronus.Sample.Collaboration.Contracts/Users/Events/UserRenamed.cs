using System.Runtime.Serialization;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Sample.Collaboration.Users.Events
{
    [DataContract(Name = "64089974-6371-4112-84dc-4326ab3ec52e")]
    public class UserRenamed : IEvent
    {
        UserRenamed() { }

        public UserRenamed(UserId id, string firstName, string lastName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
        }

        [DataMember(Order = 1)]
        public UserId Id { get; private set; }

        [DataMember(Order = 2)]
        public string FirstName { get; private set; }

        [DataMember(Order = 3)]
        public string LastName { get; private set; }
    }

    [DataContract(Name = "b4a07bd0-4347-4355-b7f2-05b74be38ba9")]
    public class EmailChanged : IEvent
    {
        EmailChanged() { }

        public EmailChanged(UserId id, string newEmail)
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