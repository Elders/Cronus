namespace Elders.Cronus.Projections.Cassandra.EventSourcing
{
    public static class EventExtensions
    {
        public static IEvent Unwrap(this IEvent @event)
        {
            var entityEvent = @event as EntityEvent;
            if (ReferenceEquals(null, entityEvent) == false)
                return entityEvent.Event;

            return @event;
        }
    }
}
