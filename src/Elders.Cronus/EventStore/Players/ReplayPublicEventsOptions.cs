using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Players
{
    [DataContract(Name = "a4a29dd3-4dfd-4b1c-941f-02760ac23576")]
    public class ReplayPublicEventsOptions
    {
        [DataMember(Order = 1)]
        public DateTimeOffset? After { get; set; }

        [DataMember(Order = 2)]
        public DateTimeOffset? Before { get; set; }

    }
}
