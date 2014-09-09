using System;
using System.Collections.Generic;
using Elders.Cronus.EventSourcing;
using System.Linq;

namespace Elders.Cronus.DomainModeling
{
    public static class AggregateRootFactory
    {
        public static TAggregateRoot Build<TAggregateRoot>(List<AggregateCommit> commits)
            where TAggregateRoot : IAggregateRoot
        {
            var ar = (TAggregateRoot)FastActivator.CreateInstance(typeof(TAggregateRoot), true);
            var events = commits.SelectMany(x => x.Events).ToList();
            var state = ar.BuildStateFromHistory(events);
            state.Version = commits.Last().Revision;
            return Build(ar, state);
        }

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