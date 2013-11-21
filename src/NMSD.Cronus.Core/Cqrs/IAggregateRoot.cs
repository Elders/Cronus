using Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.Cqrs
{
    public interface IAggregateRoot : IAggregateRootStateManager
    {
        void Apply(IEvent @event);
    }
}