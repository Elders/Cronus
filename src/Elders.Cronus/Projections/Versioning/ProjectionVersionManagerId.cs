using System.Runtime.Serialization;
using System;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "c9c8bfe1-a282-485f-87f8-e4aa6af12d56")]
    public class ProjectionVersionManagerId : StringTenantId
    {
        ProjectionVersionManagerId() : base() { }

        public ProjectionVersionManagerId(string projectionContractId) : base(projectionContractId, "projectionmanager", "elders") { }
        public ProjectionVersionManagerId(Type projectionType) : this(projectionType.GetContractId()) { }
        public ProjectionVersionManagerId(string projectionContractId, string tenant) : this(projectionContractId) { }
        public ProjectionVersionManagerId(IUrn urn) : base(urn, "projectionmanager") { }
    }
}
