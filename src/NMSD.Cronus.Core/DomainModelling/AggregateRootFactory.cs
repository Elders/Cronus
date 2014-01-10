using System;
using System.Collections.Generic;
using NMSD.Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.DomainModelling
{
    public static class AggregateRootFactory
    {
        public static TAggregateRoot Build<TAggregateRoot>(List<IEvent> events)
            where TAggregateRoot : IAggregateRoot
        {
            var ar = (TAggregateRoot)FastActivator.CreateInstance(typeof(TAggregateRoot), true);
            var state = ar.BuildStateFromHistory(events);
            return Build(ar, state);
        }

        public static TAggregateRoot Build<TAggregateRoot>(IAggregateRootState state)
            where TAggregateRoot : IAggregateRoot
        {
            var ar = (TAggregateRoot)FastActivator.CreateInstance(typeof(TAggregateRoot), true);
            return Build(ar, state);
        }

        private static TAggregateRoot Build<TAggregateRoot>(TAggregateRoot aggregateRoot, IAggregateRootState state)
            where TAggregateRoot : IAggregateRoot
        {
            if (state == null || state.Id == null || state.Id.Id == default(Guid))
                throw new AggregateRootException("Invalid aggregate root state. The initial event which created the aggregate root is missing.");

            aggregateRoot.State = state;
            return aggregateRoot;
        }
    }
}