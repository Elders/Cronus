using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestUpdateCommand : ICommand
    {
        public TestAggregateId Id { get; set; }
    }
}
