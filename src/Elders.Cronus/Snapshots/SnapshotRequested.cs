using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "7636607d-40b9-4dfe-b395-d2da71e02444")]
    public class SnapshotRequested : ISystemEvent
    {
        SnapshotRequested() { }

        public SnapshotRequested(SnapshotManagerId id, int revision, string aggregateContract, DateTimeOffset timestamp)
        {
            if (string.IsNullOrWhiteSpace(aggregateContract)) throw new ArgumentException($"'{nameof(aggregateContract)}' cannot be null or whitespace.", nameof(aggregateContract));

            Id = id ?? throw new ArgumentNullException(nameof(id));
            Revision = revision;
            AggregateContract = aggregateContract;
            Timestamp = timestamp;
        }

        [DataMember(Order = 1)]
        public SnapshotManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public int Revision { get; private set; }

        [DataMember(Order = 3)]
        public string AggregateContract { get; private set; }

        [DataMember(Order = 4)]
        public DateTimeOffset Timestamp { get; private set; }
    }

    [DataContract(Name = "e28baec5-a03e-4ab5-957d-63184c6ba85d")]
    public class SnapshotCompleted : ISystemEvent
    {
        SnapshotCompleted() { }

        public SnapshotCompleted(SnapshotManagerId id, int revision, string aggregateContract, DateTimeOffset timestamp)
        {
            if (string.IsNullOrWhiteSpace(aggregateContract)) throw new ArgumentException($"'{nameof(aggregateContract)}' cannot be null or whitespace.", nameof(aggregateContract));

            Id = id ?? throw new ArgumentNullException(nameof(id));
            Revision = revision;
            AggregateContract = aggregateContract;
            Timestamp = timestamp;
        }

        [DataMember(Order = 1)]
        public SnapshotManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public int Revision { get; private set; }

        [DataMember(Order = 3)]
        public string AggregateContract { get; private set; }

        [DataMember(Order = 4)]
        public DateTimeOffset Timestamp { get; private set; }
    }

    [DataContract(Name = "40f98848-7af4-4108-b075-1f95d90c06f0")]
    public class SnapshotFailed : ISystemEvent
    {
        SnapshotFailed() { }

        public SnapshotFailed(SnapshotManagerId id, int revision, string aggregateContract, DateTimeOffset timestamp)
        {
            if (string.IsNullOrWhiteSpace(aggregateContract)) throw new ArgumentException($"'{nameof(aggregateContract)}' cannot be null or whitespace.", nameof(aggregateContract));

            Id = id ?? throw new ArgumentNullException(nameof(id));
            Revision = revision;
            AggregateContract = aggregateContract;
            Timestamp = timestamp;
        }

        [DataMember(Order = 1)]
        public SnapshotManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public int Revision { get; private set; }

        [DataMember(Order = 3)]
        public string AggregateContract { get; private set; }

        [DataMember(Order = 4)]
        public DateTimeOffset Timestamp { get; private set; }
    }

    [DataContract(Name = "d1b92bdf-1d4a-43c9-b0c9-b0a63f336786")]
    public class SnapshotCanceled : ISystemEvent
    {
        SnapshotCanceled() { }

        public SnapshotCanceled(SnapshotManagerId id, int revision, string aggregateContract, DateTimeOffset timestamp)
        {
            if (string.IsNullOrWhiteSpace(aggregateContract)) throw new ArgumentException($"'{nameof(aggregateContract)}' cannot be null or whitespace.", nameof(aggregateContract));

            Id = id ?? throw new ArgumentNullException(nameof(id));
            Revision = revision;
            AggregateContract = aggregateContract;
            Timestamp = timestamp;
        }

        [DataMember(Order = 1)]
        public SnapshotManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public int Revision { get; private set; }

        [DataMember(Order = 3)]
        public string AggregateContract { get; private set; }

        [DataMember(Order = 4)]
        public DateTimeOffset Timestamp { get; private set; }
    }
}
