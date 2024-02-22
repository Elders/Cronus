using System.Runtime.Serialization;

namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.Bar;

[DataContract(Name = "461006d9-9845-43f6-9601-3407eab1b73b")]
public class BarId : AggregateRootId
{
    BarId() { }

    public BarId(AggregateRootId id) : base(id.Tenant, "Bar", id) { }

    public BarId(string id, string tenant) : base(tenant, "Bar", id) { }
}
