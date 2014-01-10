using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Messaging;

namespace NMSD.Cronus.Core.DomainModelling
{
    [DataContract(Name = "6a146321-7b94-4ff6-bf2c-cd82cba1836b")]
    public class DomainMessageCommit : IMessage
    {
        DomainMessageCommit()
        {
            UncommittedEvents = new List<object>();
        }

        public DomainMessageCommit(IAggregateRootState state, List<IEvent> events)
        {
            UncommittedState = state;
            UncommittedEvents = events.Cast<object>().ToList();
        }

        [DataMember(Order = 1)]
        private object UncommittedState { get; set; }

        [DataMember(Order = 2)]
        private List<object> UncommittedEvents { get; set; }

        public IAggregateRootState State { get { return (IAggregateRootState)UncommittedState; } }

        public List<IEvent> Events { get { return UncommittedEvents.Cast<IEvent>().ToList(); } }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("Message Commit Containting '{0}' Events.", Events.Count));
            foreach (var item in Events)
            {
                sb.AppendFormat("\n {0}", item.ToString());
            }
            return sb.ToString();
        }
    }

}
