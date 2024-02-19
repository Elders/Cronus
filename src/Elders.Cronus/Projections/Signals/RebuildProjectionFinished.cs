using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "b248432b-451c-4894-84f2-c5ac5bc35139")]
    public sealed class RebuildProjectionFinished : ISystemSignal
    {
        RebuildProjectionFinished()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        public RebuildProjectionFinished(string tenant, string projectionTypeId) : this()
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
