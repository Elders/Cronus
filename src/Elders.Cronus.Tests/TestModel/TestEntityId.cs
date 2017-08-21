using System;

namespace Elders.Cronus.Tests.TestModel
{

    public class TestEntityId : EntityGuidId<TestAggregateId>
    {
        public TestEntityId(Guid id, TestAggregateId rootId)
            : base(id, rootId, "TestEntityId")
        {

        }

        public TestEntityId(TestAggregateId rootId)
            : base(Guid.NewGuid(), rootId, "TestEntityId")
        {

        }
    }
}
