using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestUpdateCommand : Command
    {
        TestUpdateCommand() { }

        public TestUpdateCommand(TestAggregateId id)
            : base(id)
        {

        }

        public TestAggregateId Id { get; set; }
    }
}
