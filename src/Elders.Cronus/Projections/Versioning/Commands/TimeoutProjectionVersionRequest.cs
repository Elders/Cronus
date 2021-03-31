﻿using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "d661a3e9-f6d0-4ea7-9bd9-435d3c3b8712")]
    public class TimeoutProjectionVersionRequest : ISystemCommand
    {
        TimeoutProjectionVersionRequest() { }

        public TimeoutProjectionVersionRequest(ProjectionVersionManagerId id, ProjectionVersion version, VersionRequestTimebox timebox)
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
