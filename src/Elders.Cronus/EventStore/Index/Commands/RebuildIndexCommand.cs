using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "f9c1c15b-ed60-4fa8-ab5f-251403aa8681")]
    public class RebuildIndexCommand : ISystemCommand
    {
        RebuildIndexCommand() { }

        public RebuildIndexCommand(EventStoreIndexManagerId id)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        [DataMember(Order = 1)]
        public EventStoreIndexManagerId Id { get; private set; }
    }
}
