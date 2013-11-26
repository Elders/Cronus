using System;
using System.Collections.Generic;
using Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.Cqrs
{
    public abstract class AggregateRootState<ID> : IAggregateRootState where ID : IAggregateRootId
    {
        IAggregateRootId IAggregateRootState.Id { get { return Id; } }

        public ID Id { get; set; }

        public int Version { get; set; }

        public void Apply(IEvent @event)
        {
            var state = (dynamic)this;
            state.When((dynamic)@event);
        }

    }
}