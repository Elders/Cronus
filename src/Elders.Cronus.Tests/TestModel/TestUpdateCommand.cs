using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestUpdateCommand : ICommand
    {
        TestUpdateCommand() { }

        public TestUpdateCommand(TestAggregateId id)
        {

        }

        public TestAggregateId Id { get; set; }
    }
}
