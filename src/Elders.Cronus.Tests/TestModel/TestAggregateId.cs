using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Tests.TestModel
{
    [DataContract(Name = "9bc4ea72-575d-4577-9440-63f867f0e415")]
    public class TestAggregateId : GuidId
    {
        public TestAggregateId(Guid id)
            : base(id, "TestAggregateId")
        {

        }

        public TestAggregateId()
            : base(Guid.NewGuid(), "TestAggregateId")
        {

        }
    }
}
