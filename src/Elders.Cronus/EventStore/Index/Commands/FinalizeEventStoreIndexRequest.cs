using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "4f44a246-33a0-4314-aea2-80cd3b428b16")]
    public class FinalizeEventStoreIndexRequest : ICommand
    {
        FinalizeEventStoreIndexRequest() { }

        public FinalizeEventStoreIndexRequest(EventStoreIndexManagerId id)
        {
            Id = id;
        }

        [DataMember(Order = 1)]
        public EventStoreIndexManagerId Id { get; private set; }
    }
}
