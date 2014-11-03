using System;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Tests.TestModel
{
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