using Elders.Cronus.Projections;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "37336a18-573a-4e9e-b4a2-085033b74353")]
    public class ProjectionIndex : IEventStoreIndex
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(ProjectionIndex));

        private readonly TypeContainer<IProjection> projectionsContainer;
        private readonly IProjectionWriter projection;

        public ProjectionIndex(TypeContainer<IProjection> projectionsContainer, IProjectionWriter projection)
        {
            this.projectionsContainer = projectionsContainer;
            this.projection = projection;
        }

        public void Index(CronusMessage message)
        {
            Type baseMessageType = typeof(IMessage);
            Type messagePayloadType = message.Payload.GetType();
            foreach (var projectionType in projectionsContainer.Items)
            {
                bool isInterested = projectionType.GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericArguments().Length == 1 && (baseMessageType.IsAssignableFrom(x.GetGenericArguments()[0])))
                    .Where(@interface => @interface.GetGenericArguments()[0].IsAssignableFrom(messagePayloadType))
                    .Any();

                if (isInterested)
                {
                    projection.Save(projectionType, message);
                }
            }
        }
    }
}
