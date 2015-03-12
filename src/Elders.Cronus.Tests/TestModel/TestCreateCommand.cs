using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestCreateCommand : ICommand
    {
        TestCreateCommand() { }

        public TestCreateCommand(TestAggregateId id)
        {

        }

        public TestAggregateId Id { get; set; }
    }
}
