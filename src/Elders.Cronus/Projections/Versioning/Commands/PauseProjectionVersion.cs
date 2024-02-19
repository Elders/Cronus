using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "e78cc312-23d8-49b4-b06e-cb86661634c7")]
    public sealed class PauseProjectionVersion : ISystemCommand
    {
        PauseProjectionVersion()
        {
            Timestamp = DateTimeOffset.Now;
        }

        public PauseProjectionVersion(ProjectionVersionManagerId id, ProjectionVersion version) : this()
        {
            if (id is null) throw new ArgumentNullException(nameof(id));
            if (version is null) throw new ArgumentNullException(nameof(version));

            Id = id;
            Version = version;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion Version { get; private set; }

        [DataMember(Order = 3)]
        public DateTimeOffset Timestamp { get; private set; }

        public override string ToString()
        {
            return $"Pause projection version with hash `{Version}`. {nameof(ProjectionVersionManagerId)}: {Id}";
        }
    }
}
