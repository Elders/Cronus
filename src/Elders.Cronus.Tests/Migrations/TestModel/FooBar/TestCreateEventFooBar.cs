using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.FooBar
{
    [DataContract(Name = "58c3f873-73dc-4592-a9f4-370b4bb23395")]
    public class TestCreateEventFooBar : IEvent
    {
        public TestCreateEventFooBar(FooBarId id)
        {
            Id = id;
        }

        [DataMember(Order = 1)]
        public FooBarId Id { get; set; }

        public DateTimeOffset Timestamp => DateTimeOffset.UtcNow;
    }
}
