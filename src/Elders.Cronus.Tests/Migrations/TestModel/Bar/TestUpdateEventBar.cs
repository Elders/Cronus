using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.Bar
{
    [DataContract(Name = "aced4ce6-29b1-40e1-8fa8-4f6a67721383")]
    public class TestUpdateEventBar : IEvent
    {
        public TestUpdateEventBar(BarId id, string updatedFieldValue)
        {
            Id = id;
            UpdatedFieldValue = updatedFieldValue;
        }

        [DataMember(Order = 1)]
        public BarId Id { get; set; }
        [DataMember(Order = 2)]
        public string UpdatedFieldValue { get; set; }

        public DateTimeOffset Timestamp => DateTimeOffset.UtcNow;
    }
}
