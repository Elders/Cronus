using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Tests.TestModel
{
    public class TestCreateCommand : ICommand
    {
        public TestAggregateId Id { get; set; }
    }
}
