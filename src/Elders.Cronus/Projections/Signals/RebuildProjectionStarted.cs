using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "b03199e7-2752-48b7-93de-c45ad18b55bf")]
    public sealed class RebuildProjectionStarted : ISystemSignal
    {
        RebuildProjectionStarted()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        public RebuildProjectionStarted(string tenant, string projectionTypeId) : this()
        {
            Tenant = tenant;
            ProjectionTypeId = projectionTypeId;
        }

        [DataMember(Order = 0)]
        public string Tenant { get; private set; }

        [DataMember(Order = 1)]
        public string ProjectionTypeId { get; private set; }

        [DataMember(Order = 2)]
        public DateTimeOffset Timestamp { get; private set; }
    }
}
