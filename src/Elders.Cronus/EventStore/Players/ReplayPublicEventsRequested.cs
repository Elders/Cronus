using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Players
{
    [DataContract(Name = "6574cff5-9195-4183-9f98-83e80da842cb")]
    public class ReplayPublicEventsRequested : ISystemSignal
    {
        [DataMember(Order = 0)]
        public string Tenant { get; set; }

        [DataMember(Order = 1)]
        public string SourceEventTypeId { get; set; }

        [DataMember(Order = 2)]
        public string RecipientBoundedContext { get; set; }

        [DataMember(Order = 3)]
        public string RecipientHandlers { get; set; }

        [DataMember(Order = 4)]
        public ReplayEventsOptions ReplayOptions { get; set; }
    }
}
