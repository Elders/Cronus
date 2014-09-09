using System;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestAggregateId : AggregateRootId
    {
        public TestAggregateId(Guid id)
            : base(id)
        {

        }

        public TestAggregateId()
            : base(Guid.NewGuid())
        {

        }
    }
}