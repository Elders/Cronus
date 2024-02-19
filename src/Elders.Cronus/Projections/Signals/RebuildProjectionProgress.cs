using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "373f4ff0-cb6f-499e-9fa5-1666ccc00689")]
    public sealed class RebuildProjectionProgress : ISystemSignal
    {
        RebuildProjectionProgress()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        public RebuildProjectionProgress(string tenant, string projectionTypeId, ulong processedCount, ulong totalCount) : this()
        {
            Tenant = tenant;
            ProjectionTypeId = projectionTypeId;
            ProcessedCount = processedCount;
            TotalCount = totalCount;
        }

        [DataMember(Order = 0)]
        public string Tenant { get; set; }

        [DataMember(Order = 1)]
        public string ProjectionTypeId { get; set; }

        [DataMember(Order = 2)]
        public ulong ProcessedCount { get; set; }

        [DataMember(Order = 3)]
        public ulong TotalCount { get; set; }

        [DataMember(Order = 4)]
        public DateTimeOffset Timestamp { get; set; }
    }
}
