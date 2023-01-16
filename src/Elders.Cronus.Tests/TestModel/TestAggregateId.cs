using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Tests.TestModel
{
    [DataContract(Name = "9bc4ea72-575d-4577-9440-63f867f0e415")]
    public class TestAggregateId : AggregateRootId
    {
        public TestAggregateId(Guid id)
            : base("cronustest", "TestAggregateId", id.ToString())
        {

        }

        public TestAggregateId()
            : base("cronustest", "TestAggregateId", Guid.NewGuid().ToString())
        {

        }
    }
}
