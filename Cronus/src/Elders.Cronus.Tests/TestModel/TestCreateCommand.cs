using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestCreateCommand : Command
    {
        TestCreateCommand() { }

        public TestCreateCommand(TestAggregateId id)
            : base(id)
        {

        }

        public TestAggregateId Id { get; set; }
    }
}
