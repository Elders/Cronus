using System;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections
{
    public interface IProjectionWriter
    {
        void Save(Type projectionType, CronusMessage cronusMessage);
        void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin);
        void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin, ProjectionVersion version);

        Task SaveAsync(Type projectionType, CronusMessage cronusMessage);
        Task SaveAsync(Type projectionType, IEvent @event, EventOrigin eventOrigin);
        Task SaveAsync(Type projectionType, IEvent @event, EventOrigin eventOrigin, ProjectionVersion version);
    }
}
