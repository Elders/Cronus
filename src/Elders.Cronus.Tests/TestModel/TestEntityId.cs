using System;

namespace Elders.Cronus.Tests.TestModel
{

    public class TestEntityId : EntityStringId<TestAggregateId>
    {
        public TestEntityId(Guid id, TestAggregateId rootId)
            : base(id.ToString(), rootId, "TestEntityId")
        {

        }

        public TestEntityId(TestAggregateId rootId)
            : base(Guid.NewGuid().ToString(), rootId, "TestEntityId")
        {

        }
    }
}
