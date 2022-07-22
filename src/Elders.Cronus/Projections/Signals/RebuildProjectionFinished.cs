using System.Runtime.Serialization;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "b248432b-451c-4894-84f2-c5ac5bc35139")]
    public class RebuildProjectionFinished : ISystemSignal
    {
        public RebuildProjectionFinished() { }

        public RebuildProjectionFinished(string tenant, string projectionTypeId)
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
