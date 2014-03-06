using System.Runtime.Serialization;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Sample.Collaboration.Users.Events;

namespace NMSD.Cronus.Sample.Collaboration.Users
{
    [DataContract(Name = "c8978654-4380-44d2-8ebe-ae17a463dfb6")]
    public class UserState : AggregateRootState<UserId>
    {
        public UserState() { }

        [DataMember(Order = 1)]
        public override UserId Id { get; set; }

        [DataMember(Order = 2)]
        public override int Version { get; set; }

        [DataMember(Order = 3)]
        public string Email { get; private set; }

        [DataMember(Order = 4)]
        public string Firstname { get; private set; }

        [DataMember(Order = 5)]
        public string LastName { get; private set; }

        public void When(UserRenamed e)
        {
            Firstname = e.FirstName;
            LastName = e.LastName;
        }

        public void When(UserCreated e)
        {
            Id = e.CollaboratorId;
            Email = e.Email;
        }
    }
}