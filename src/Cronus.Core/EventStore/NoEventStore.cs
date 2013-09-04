namespace Cronus.Core.EventStore
{
    public class NoEventStore : IEventStore
    {
        public void Save(Eventing.IEvent @event) { }
    }
}