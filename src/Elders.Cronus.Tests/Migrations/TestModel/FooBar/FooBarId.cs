using System.Runtime.Serialization;

namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.FooBar
{
    [DataContract(Name = "08435f7b-cc0b-44eb-b88f-c6d60419c2b8")]
    public class FooBarId : AggregateRootId
    {
        FooBarId() { }

        public FooBarId(AggregateRootId id) : base(id.Tenant, "FooBar", id) { }

        public FooBarId(string id, string tenant) : base(tenant, "FooBar", id) { }
    }
}
