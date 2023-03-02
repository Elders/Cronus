using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "7636607d-40b9-4dfe-b395-d2da71e02444")]
    public class SnapshotRequested : ISystemEvent
    {
        SnapshotRequested() { }

        public SnapshotRequested(SnapshotManagerId id, int revision, string aggregareContract, DateTimeOffset timestamp)
        {
            if (string.IsNullOrWhiteSpace(aggregareContract)) throw new ArgumentException($"'{nameof(aggregareContract)}' cannot be null or whitespace.", nameof(aggregareContract));

            Id = id ?? throw new ArgumentNullException(nameof(id));
            Revision = revision;
            AggregareContract = aggregareContract;
            Timestamp = timestamp;
        }

        [DataMember(Order = 1)]
        public SnapshotManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public int Revision { get; private set; }

        [DataMember(Order = 3)]
        public string AggregareContract { get; private set; }

        [DataMember(Order = 4)]
        public DateTimeOffset Timestamp { get; private set; }
    }
}
