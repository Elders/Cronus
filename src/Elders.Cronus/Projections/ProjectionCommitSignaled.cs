using System.Runtime.Serialization;
using System;

namespace Elders.Cronus.Projections
{
    [DataContract(Namespace = "cronus", Name = "7ddf8796-7086-4cea-8478-7fd11684066e")]
    public class ProjectionCommitSignaled : ISystemSignal
    {
        public ProjectionCommitSignaled() { }

        public ProjectionCommitSignaled(string tenant, IBlobId projectionId, Type projectionType, ProjectionVersion version)
        {
            Tenant = tenant;
            ProjectionId = projectionId;
            ProjectionType = projectionType;
            Version = version;
        }

        [DataMember(Order = 0)]
        public string Tenant { get; private set; }

        [DataMember(Order = 1)]
        public IBlobId ProjectionId { get; private set; }

        [DataMember(Order = 2)]
        public Type ProjectionType { get; private set; }

        [DataMember(Order = 3)]
        public ProjectionVersion Version { get; private set; }
    }
}
