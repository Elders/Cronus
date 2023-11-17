using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo
{
    [DataContract(Name = "85ae53dd-8b4e-48dd-998b-7b00a77e1530")]
    public class TestCreateEventFoo : IEvent
    {
        public TestCreateEventFoo(FooId id)
        {
            Id = id;
        }

        [DataMember(Order = 1)]
        public FooId Id { get; set; }

        public DateTimeOffset Timestamp => DateTimeOffset.UtcNow;
    }
}
