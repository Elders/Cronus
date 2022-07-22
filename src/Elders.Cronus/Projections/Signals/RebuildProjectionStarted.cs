using System.Runtime.Serialization;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "b03199e7-2752-48b7-93de-c45ad18b55bf")]
    public class RebuildProjectionStarted : ISystemSignal
    {
        public RebuildProjectionStarted() { }

        public RebuildProjectionStarted(string tenant, string projectionTypeId)
        {
            Tenant = tenant;
            ProjectionTypeId = projectionTypeId;
        }

        [DataMember(Order = 0)]
        public string Tenant { get; set; }

        [DataMember(Order = 1)]
        public string ProjectionTypeId { get; set; }
    }
}
