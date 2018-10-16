using System;

namespace Elders.Cronus.Projections
{
    public interface IProjectionWriter
    {
        void Save(Type projectionType, CronusMessage cronusMessage);
        void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin);
        void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin, ProjectionVersion version);
    }
}
