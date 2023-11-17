using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.FooBar
{
    [DataContract(Name = "c9cd0672-ece9-4ce8-a8d6-a81c82a1d78a")]
    public class TestUpdateEventFooBar : IEvent
    {
        public TestUpdateEventFooBar(FooBarId id, string updatedFieldValue)
        {
            Id = id;
            UpdatedFieldValue = updatedFieldValue;
        }

        [DataMember(Order = 1)]
        public FooBarId Id { get; set; }
        [DataMember(Order = 2)]
        public string UpdatedFieldValue { get; set; }

        public DateTimeOffset Timestamp => DateTimeOffset.UtcNow;
    }
}
