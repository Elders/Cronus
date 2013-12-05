using System.Runtime.Serialization;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators.Commands
{
    [DataContract(Name = "aeaae173-d790-443d-92b2-caa06d55f1a2")]
    public class RenameCollaborator : ICommand
    {
        RenameCollaborator() { }

        public RenameCollaborator(CollaboratorId id, string firstName, string lastName)
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

    [DataContract(Name = "145e03ca-436b-4343-a54b-94a4236dbf39")]
    public class TestRenameCollaborator : ICommand
    {
        TestRenameCollaborator() { }

        public TestRenameCollaborator(CollaboratorId id, string firstName, string lastName)
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