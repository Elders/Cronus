using System;
using System.Collections.Generic;
using Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.Cqrs
{
    public static class AggregateRootFactory
    {
        public static AR BuildFromHistory<AR>(List<IEvent> events)
            where AR : IAggregateRoot
        {
            var ar = (AR)Activator.CreateInstance(typeof(AR), true);
            var state = ar.BuildFromHistory(events);
            if (state == null || state.Id == null || state.Id.Id == default(Guid))
                throw new AggregateRootException("Invalid aggregate root state. The initial event which created the aggregate root is missing.");
            ar.State = state;
            return ar;
        }

    }
}