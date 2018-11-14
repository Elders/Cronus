using System.Runtime.Serialization;

namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.FooBar
{
    [DataContract(Name = "08435f7b-cc0b-44eb-b88f-c6d60419c2b8")]
    public class FooBarId : StringTenantId
    {
        FooBarId() { }

        public FooBarId(StringTenantId id) : base(id, "FooBar") { }

        public FooBarId(string id, string tenant) : base(id, "FooBar", tenant) { }
    }
}