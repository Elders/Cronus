using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "7fc355fb-9e33-4bf7-8620-9fe645eba8f4")]
    public class CreateSnapshotScheduledMessage : ISystemScheduledMessage
    {
        CreateSnapshotScheduledMessage() { }

        public CreateSnapshotScheduledMessage(SnapshotRequested snapshotRequested, DateTime publishAt)
        {
            SnapshotRequested = snapshotRequested;
            PublishAt = publishAt;
        }

        [DataMember(Order = 1)]
        public SnapshotRequested SnapshotRequested { get; private set; }

        [DataMember(Order = 2)]
        public DateTime PublishAt { get; set; }

        public string Tenant { get { return SnapshotRequested.Id.Tenant; } }
    }
}
