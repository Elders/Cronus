using System;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Tests.TestModel
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