using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo
{
    [DataContract(Name = "58fb718f-df68-4ff2-9e75-90f7bfd06c41")]
    public class TestUpdateEventFoo : IEvent
    {
        public TestUpdateEventFoo(FooId id, string updatedFieldValue)
        {
            Id = id;
            UpdatedFieldValue = updatedFieldValue;
        }

        [DataMember(Order = 1)]
        public FooId Id { get; set; }
        [DataMember(Order = 2)]
        public string UpdatedFieldValue { get; set; }

        public DateTimeOffset Timestamp => DateTimeOffset.UtcNow;
    }
}
