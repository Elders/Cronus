using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Tests.TestModel
{
    public class TestUpdateCommand : ICommand
    {
        public TestAggregateId Id { get; set; }
    }
}
