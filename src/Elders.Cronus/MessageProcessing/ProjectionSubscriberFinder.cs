using Elders.Cronus.Projections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.MessageProcessing
{
    public class ProjectionSubscriberFinder : ISubscriberFinder<IProjection>
    {
        private readonly TypeContainer<IProjection> typeContainer;

        public ProjectionSubscriberFinder(TypeContainer<IProjection> typeContainer)
        {
            this.typeContainer = typeContainer;
        }

        public IEnumerable<Type> Find()
        {
            return typeContainer.Items.Where(x => typeof(IProjectionDefinition).IsAssignableFrom(x) == false);
        }
    }
}
