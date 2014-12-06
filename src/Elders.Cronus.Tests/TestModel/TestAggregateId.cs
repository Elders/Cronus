using System;
using Elders.Cronus.DomainModeling;
using System.Runtime.Serialization;

namespace Elders.Cronus.Tests.TestModel
{
    [DataContract(Name = "bb2daebe-ef38-4648-b92a-204ac4ceae72")]
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