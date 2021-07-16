using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "8486d01e-cf44-44d4-92fe-a97f3205bfb5")]
    public class TimeoutProjectionRebuildRequest : ISystemCommand
    {
        TimeoutProjectionRebuildRequest() { }

        public TimeoutProjectionRebuildRequest(ProjectionVersionManagerId id, ProjectionVersion version, VersionRequestTimebox timebox)
        {
            Id = id;
            Version = version;
            Timebox = timebox;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion Version { get; private set; }

        [DataMember(Order = 3)]
        public VersionRequestTimebox Timebox { get; private set; }

        public override string ToString()
        {
            return $"Timeout projection rebuilding for version `{Version}`. {Environment.NewLine}{nameof(ProjectionVersionManagerId)}: `{Id}`. {Environment.NewLine}Timebox: `{Timebox}`.";
        }
    }
}
