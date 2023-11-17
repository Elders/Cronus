using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Migration.Middleware.Tests.TestModel.Bar
{
    [DataContract(Name = "edb7ad37-cdcf-4a2e-b226-fe50e0d9095d")]
    public class TestCreateEventBar : IEvent
    {
        public TestCreateEventBar(BarId id)
        {
            Id = id;
        }

        [DataMember(Order = 1)]
        public BarId Id { get; set; }

        public DateTimeOffset Timestamp => DateTimeOffset.UtcNow;
    }
}
