using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "6e5cfbbb-8c0f-42f3-8772-052d4198c0c8")]
    internal sealed class RequestSnapshot : ISystemCommand
    {
        RequestSnapshot() { }

        public RequestSnapshot(SnapshotManagerId id, int revision, string aggregareContract)
        {
            if (revision <= 0) throw new ArgumentOutOfRangeException(nameof(revision), "Revision must be a positive number.");
            if (string.IsNullOrWhiteSpace(aggregareContract)) throw new ArgumentException($"'{nameof(aggregareContract)}' cannot be null or whitespace.", nameof(aggregareContract));

            Id = id ?? throw new ArgumentNullException(nameof(id));
            Revision = revision;
            AggregareContract = aggregareContract;
        }

        [DataMember(Order = 1)]
        public SnapshotManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public int Revision { get; private set; }

        [DataMember(Order = 3)]
        public string AggregareContract { get; private set; }
    }
}
