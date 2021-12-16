namespace Elders.Cronus.Projections.Cassandra.EventSourcing
{
    public static class EventExtensions
    {
        public static IEvent Unwrap(this IEvent @event)
        {
            if (@event is EntityEvent entityEvent)
                return entityEvent.Event;

            return @event;
        }
    }
}
