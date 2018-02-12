using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "c0ea19c2-3630-41de-a3e7-54811a5b5a30")]
    public class FinalizeProjectionVersionRequest : ICommand
    {
        FinalizeProjectionVersionRequest() { }

        public FinalizeProjectionVersionRequest(ProjectionVersionManagerId id, ProjectionVersion version)
        {
            Id = id;
            Version = version;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public ProjectionVersion Version { get; private set; }
    }

    [DataContract(Name = "d661a3e9-f6d0-4ea7-9bd9-435d3c3b8712")]
    public class TimeoutProjectionVersionRequest : ICommand
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
    }

}
