using System;

namespace Elders.Cronus.Projections
{
    public interface IProjectionRepository : IProjectionLoader
    {
        void Save(Type projectionType, CronusMessage cronusMessage);
        void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin);
    }
}
