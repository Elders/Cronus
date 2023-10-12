using System;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections
{
    public interface IProjectionWriter
    {
        Task SaveAsync(Type projectionType, IEvent @event);
        Task SaveAsync(Type projectionType, IEvent @event, ProjectionVersion version);
    }
}
