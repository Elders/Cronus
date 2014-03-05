using System.Runtime.Serialization;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Sample.Collaboration.Users.Commands
{
    [DataContract(Name = "aeaae173-d790-443d-92b2-caa06d55f1a2")]
    public class RenameUser : ICommand
    {
        RenameUser() { }

        public RenameUser(CollaboratorId id, string firstName, string lastName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
        }

        [DataMember(Order = 1)]
        public CollaboratorId Id { get; set; }

        [DataMember(Order = 2)]
        public string FirstName { get; set; }

        [DataMember(Order = 3)]
        public string LastName { get; set; }
    }
}