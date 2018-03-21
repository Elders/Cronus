using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "c9c8bfe1-a282-485f-87f8-e4aa6af12d56")]
    public class ProjectionVersionManagerId : StringTenantId
    {
        ProjectionVersionManagerId() : base() { }

        public ProjectionVersionManagerId(string projectionName) : base(projectionName, "projectionmanager", CronusAssembly.EldersTenant) { }
        public ProjectionVersionManagerId(IUrn urn) : base(urn, "projectionmanager") { }
    }
}
