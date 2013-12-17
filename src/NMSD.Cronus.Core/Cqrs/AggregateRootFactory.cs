using System;
using System.Collections.Generic;
using Cronus.Core;
using NMSD.Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.Cqrs
{
    public static class AggregateRootFactory
    {
        public static AR Build<AR>(List<IEvent> events)
            where AR : IAggregateRoot
        {
            var ar = (AR)FastActivator.CreateInstance(typeof(AR), true);
            var state = ar.BuildStateFromHistory(events);
            return Build(ar, state);
        }

        public static AR Build<AR>(IAggregateRootState state)
            where AR : IAggregateRoot
        {
            var ar = (AR)FastActivator.CreateInstance(typeof(AR), true);
            return Build(ar, state);
        }

        private static AR Build<AR>(AR aggregateRoot, IAggregateRootState state)
            where AR : IAggregateRoot
        {
            if (state == null || state.Id == null || state.Id.Id == default(Guid))
                throw new AggregateRootException("Invalid aggregate root state. The initial event which created the aggregate root is missing.");

            aggregateRoot.State = state;
            return aggregateRoot;
        }
    }
}