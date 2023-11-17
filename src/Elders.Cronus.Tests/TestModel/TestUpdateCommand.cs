using System;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestUpdateCommand : ICommand
    {
        TestUpdateCommand() { }

        public TestUpdateCommand(TestAggregateId id)
        {

        }

        public TestAggregateId Id { get; set; }

        public DateTimeOffset Timestamp => DateTimeOffset.UtcNow;
    }
}
