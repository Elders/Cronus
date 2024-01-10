using System;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections
{
    public class ProjectionsOperator
    {
        public Func<IEvent, Task> OnProjectionEventLoadedAsync { get; set; }

        public Func<ProjectionStream, Task> OnProjectionStreamLoadedAsync { get; set; }
    }
}
