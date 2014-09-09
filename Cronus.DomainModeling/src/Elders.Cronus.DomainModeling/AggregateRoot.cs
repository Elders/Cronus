using System.Collections.Generic;

namespace Elders.Cronus.DomainModeling
{
    public class AggregateRoot<ST> : IAggregateRoot
        where ST : IAggregateRootState, new()
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
            var stateFromHistory = new ST();
            foreach (IEvent @event in events)
            {
                stateFromHistory.Apply(@event);
            }
            return stateFromHistory;
        }

    }
}