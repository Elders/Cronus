using Elders.Cronus.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "37336a18-573a-4e9e-b4a2-085033b74353")]
    public class ProjectionIndex : ICronusEventStoreIndex
    {
        private readonly TypeContainer<IProjection> projectionsContainer;
        private readonly IProjectionWriter projection;
        private readonly IEventStorePlayer eventStore;

        public ProjectionIndex(TypeContainer<IProjection> projectionsContainer, IEventStorePlayer eventStore, IProjectionWriter projection)
        {
            this.projectionsContainer = projectionsContainer;
            this.projection = projection;
            this.eventStore = eventStore;
        }

        public async Task IndexAsync(CronusMessage message)
        {
            IEnumerable<Type> projectionTypes = projectionsContainer.Items;
            if (message.IsRepublished)
                projectionTypes = message.RecipientHandlers.Intersect(projectionsContainer.Items.Select(t => t.GetContractId())).Select(dc => dc.GetTypeByContract());

            Type messagePayloadType = message.Payload.GetType();
            foreach (var projectionType in projectionTypes)
            {
                bool isInterested = projectionType.GetInterfaces()
                    .Where(@interface => IsInterested(@interface, messagePayloadType))
                    .Any();

                if (isInterested)
                {
                    await projection.SaveAsync(projectionType, message).ConfigureAwait(false);
                }
            }
        }

        public async Task<string> IndexAsync(IndexRecord indexRecord, ProjectionVersion version)
        {
            IEvent @event = await eventStore.LoadEventWithRebuildProjectionAsync(indexRecord).ConfigureAwait(false);

            Type messagePayloadType = @event.GetType();

            IEnumerable<Type> projectionTypes = projectionsContainer.Items.Where(projectionType => projectionType.GetContractId().Equals(version.ProjectionName, StringComparison.OrdinalIgnoreCase));
            foreach (var projectionType in projectionTypes)
            {
                bool isInterested = projectionType.GetInterfaces()
                                    .Where(@interface => IsInterested(@interface, messagePayloadType))
                                    .Any();

                if (isInterested)
                {
                    var origin = new EventOrigin(indexRecord.AggregateRootId, indexRecord.Revision, indexRecord.Position, indexRecord.TimeStamp);
                    await projection.SaveAsync(projectionType, @event, origin, version).ConfigureAwait(false);
                }
            }

            return messagePayloadType.GetContractId();
        }

        private static bool IsInterested(Type handlerInterfaces, Type messagePayloadType)
        {
            var genericArguments = handlerInterfaces.GetGenericArguments();

            return handlerInterfaces.IsGenericType && genericArguments.Length == 1 && messagePayloadType.IsAssignableFrom(genericArguments[0]);
        }
    }
}
