using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using NMSD.Cronus.Commanding;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.Messaging;

namespace NMSD.Cronus.DomainModelling
{
    [DataContract(Name = "6a146321-7b94-4ff6-bf2c-cd82cba1836b")]
    public class DomainMessageCommit : IMessage
    {
        DomainMessageCommit()
        {
            UncommittedEvents = new List<object>();
        }

        public DomainMessageCommit(IAggregateRootState state, List<IEvent> events, ICommand command)
        {
            CommanWrap = command;
            UncommittedState = state;
            UncommittedEvents = events.Cast<object>().ToList();
        }

        [DataMember(Order = 1)]
        private object UncommittedState { get; set; }

        [DataMember(Order = 2)]
        private List<object> UncommittedEvents { get; set; }

        [DataMember(Order = 3)]
        private object CommanWrap { get; set; }

        public IAggregateRootState State { get { return (IAggregateRootState)UncommittedState; } }

        public List<IEvent> Events { get { return UncommittedEvents.Cast<IEvent>().ToList(); } }


        public ICommand Command { get { return (ICommand)CommanWrap; } }

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
