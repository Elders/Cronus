﻿using System.Runtime.Serialization;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "373f4ff0-cb6f-499e-9fa5-1666ccc00689")]
    public class RebuildProjectionProgress : ISignal
    {
        public RebuildProjectionProgress() { }

        public RebuildProjectionProgress(string tenant, string projectionTypeId, long processedCount, long totalCount)
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
        public long ProcessedCount { get; set; }

        [DataMember(Order = 3)]
        public long TotalCount { get; set; }
    }
}