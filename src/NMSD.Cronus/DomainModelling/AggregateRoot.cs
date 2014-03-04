using System.Collections.Generic;
using NMSD.Cronus.Eventing;

namespace NMSD.Cronus.DomainModelling
{
    public class AggregateRoot<ST> : IAggregateRoot
        where ST : IAggregateRootState
    {
        protected ST state;

        public AggregateRoot()
        {
            UncommittedEvents = new List<IEvent>();
        }

        IAggregateRootState IAggregateRootStateManager.State
        {
            get { return state; }
            set { state = (ST)value; }
        }

        public List<IEvent> UncommittedEvents { get; set; }

        public void Apply(IEvent @event)
        {
            state.Apply(@event);
            UncommittedEvents.Add(@event);
        }

        IAggregateRootState IAggregateRootStateManager.BuildStateFromHistory(List<IEvent> events)
        {
            var stateFromHistory = (ST)FastActivator.CreateInstance(typeof(ST));
            foreach (IEvent @event in events)
            {
                stateFromHistory.Apply(@event);
            }
            return stateFromHistory;
        }

    }
}