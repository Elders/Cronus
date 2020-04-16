using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "64ed4d38-d197-4bdd-b0eb-a1f740c33832")]
    public class RegisterIndex : ICommand
    {
        RegisterIndex() { }

        public RegisterIndex(EventStoreIndexManagerId id)
        {
            Id = id;
        }

        [DataMember(Order = 1)]
        public EventStoreIndexManagerId Id { get; private set; }
    }
}
