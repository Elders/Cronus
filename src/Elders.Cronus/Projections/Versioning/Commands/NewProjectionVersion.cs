using Elders.Cronus.EventStore.Players;
using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Namespace = "cronus", Name = "9b309ab7-3fac-4cf8-97e2-d2e74fbaa623")]
    public sealed class NewProjectionVersion : ISystemCommand
    {
        NewProjectionVersion()
        {
            Timestamp = DateTimeOffset.Now;
        }

        public NewProjectionVersion(ProjectionVersionManagerId id, string hash, ReplayEventsOptions replayEventsOptions) : this()
        {
            if (id is null) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(hash)) throw new ArgumentNullException(nameof(hash));

            Id = id;
            Hash = hash;
            ReplayEventsOptions = replayEventsOptions;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public string Hash { get; private set; }

        [DataMember(Order = 3)]
        public ReplayEventsOptions ReplayEventsOptions { get; private set; }

        [DataMember(Order = 4)]
        public DateTimeOffset Timestamp { get; private set; }

        public override string ToString()
        {
            return $"Rebuild projection with hash `{Hash}`. {nameof(ProjectionVersionManagerId)}: `{Id}`";
        }
    }
}
