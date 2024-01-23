using Elders.Cronus.EventStore.Players;
using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Namespace = "cronus", Name = "d54d6def-2c33-4d19-9009-26ae357d6fc2")]
    public sealed class RegisterProjection : ISystemCommand
    {
        RegisterProjection()
        {
            ReplayEventsOptions = new ReplayEventsOptions();
            Timestamp = DateTimeOffset.UtcNow;
        }

        public RegisterProjection(ProjectionVersionManagerId id, string hash) : this()
        {
            if (id is null) throw new ArgumentNullException(nameof(id));
            // if (string.IsNullOrEmpty(hash)) throw new ArgumentNullException(nameof(hash));

            Id = id;
            Hash = hash;
            ReplayEventsOptions = new ReplayEventsOptions();
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
            return $"Register projection version with {Hash}. {nameof(ProjectionVersionManagerId)}: {Id}.";
        }
    }
}
