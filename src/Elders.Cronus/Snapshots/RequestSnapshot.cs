using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "6e5cfbbb-8c0f-42f3-8772-052d4198c0c8")]
    internal sealed class RequestSnapshot : ISystemCommand
    {
        RequestSnapshot() { }

        public RequestSnapshot(SnapshotManagerId id, int revision, string contract, int eventsLoaded, TimeSpan loadTime)
        {
            if (revision <= 0) throw new ArgumentOutOfRangeException(nameof(revision), "Revision must be a positive number.");
            if (string.IsNullOrWhiteSpace(contract)) throw new ArgumentException($"'{nameof(contract)}' cannot be null or whitespace.", nameof(contract));

            Id = id ?? throw new ArgumentNullException(nameof(id));
            Revision = revision;
            Contract = contract;
            EventsLoaded = eventsLoaded;
            LoadTime = loadTime;
        }

        [DataMember(Order = 1)]
        public SnapshotManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public int Revision { get; private set; }

        [DataMember(Order = 3)]
        public string Contract { get; private set; }

        [DataMember(Order = 4)]
        public int EventsLoaded { get; private set; }

        [DataMember(Order = 5)]
        public TimeSpan LoadTime { get; private set; }
    }
}
