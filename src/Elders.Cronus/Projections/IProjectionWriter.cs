using System;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections
{
    public interface IProjectionWriter
    {
        Task SaveAsync(Type projectionType, CronusMessage cronusMessage);
        Task SaveAsync(Type projectionType, IEvent @event, EventOrigin eventOrigin);
        Task SaveAsync(Type projectionType, IEvent @event, EventOrigin eventOrigin, ProjectionVersion version);
    }
}
