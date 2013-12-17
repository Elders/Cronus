using System.Runtime.Serialization;
using NMSD.Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.Cqrs
{
    public abstract class AggregateRootState<ID> : IAggregateRootState
        where ID : IAggregateRootId
    {
        IAggregateRootId IAggregateRootState.Id { get { return Id; } }

        public abstract ID Id { get; set; }

        public abstract int Version { get; set; }

        public void Apply(IEvent @event)
        {
            var state = (dynamic)this;
            state.When((dynamic)@event);
        }

    }
}