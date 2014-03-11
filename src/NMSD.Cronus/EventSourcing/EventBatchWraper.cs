using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NMSD.Cronus.EventSourcing
{
    [DataContract(Name = "987a7bed-7689-4c08-b610-9a802d306215")]
    public class EventBatchWraper
    {
        EventBatchWraper() { }

        public EventBatchWraper(List<object> events)
        {
            Events = events;
        }

        [DataMember(Order = 1)]
        public List<object> Events { get; private set; }

    }
}