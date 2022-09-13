using Elders.Cronus.Projections;
using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ProjectionIndex> logger;
        private readonly TypeContainer<IProjection> projectionsContainer;
        private readonly IProjectionWriter projection;
        private readonly IEventStorePlayer eventStore;

        public ProjectionIndex(TypeContainer<IProjection> projectionsContainer, IEventStorePlayer eventStore, IProjectionWriter projection, ILogger<ProjectionIndex> logger)
        {
            this.projectionsContainer = projectionsContainer;
            this.projection = projection;
            this.logger = logger;
            this.eventStore = eventStore;
        }

        public async Task IndexAsync(CronusMessage message)
        {
            IEnumerable<Type> projectionTypes = projectionsContainer.Items;
            if (message.IsRepublished)
                projectionTypes = message.RecipientHandlers.Intersect(projectionsContainer.Items.Select(t => t.GetContractId())).Select(dc => dc.GetTypeByContract());

            Type baseMessageType = typeof(IMessage);
            Type messagePayloadType = message.Payload.GetType();
            foreach (var projectionType in projectionTypes)
            {
                bool isInterested = projectionType.GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericArguments().Length == 1 && (baseMessageType.IsAssignableFrom(x.GetGenericArguments()[0])))
                    .Where(@interface => @interface.GetGenericArguments()[0].IsAssignableFrom(messagePayloadType))
                    .Any();

                if (isInterested)
                {
                    await projection.SaveAsync(projectionType, message).ConfigureAwait(false);
                }
            }
        }

        /*        public async Task IndexAsync(AggregateCommit aggregateCommit, ProjectionVersion version)
                {
                    Type baseMessageType = typeof(IMessage);

                    IEnumerable<(IEvent, EventOrigin)> eventDataList = GetEventData(aggregateCommit);

                    foreach (var eventData in eventDataList)
                    {
                        Type messagePayloadType = eventData.Item1.GetType();
                        foreach (var projectionType in projectionsContainer.Items)
                        {
                            if (projectionType.GetContractId().Equals(version.ProjectionName, StringComparison.OrdinalIgnoreCase) == false)
                                continue;

                            bool isInterested = projectionType.GetInterfaces()
                                .Where(x => x.IsGenericType && x.GetGenericArguments().Length == 1 && (baseMessageType.IsAssignableFrom(x.GetGenericArguments()[0])))
                                .Where(@interface => @interface.GetGenericArguments()[0].IsAssignableFrom(messagePayloadType))
                                .Any();

                            if (isInterested)
                            {
                                await projection.SaveAsync(projectionType, eventData.Item1, eventData.Item2, version).ConfigureAwait(false);
                            }
                        }
                    }
                }*/


        public async Task<string> IndexAsync(IndexRecord indexRecord, ProjectionVersion version)
        {
            IEvent @event = await eventStore.LoadEventWithRebuildProjectionAsync(indexRecord).ConfigureAwait(false);
            
            Type messagePayloadType = @event.GetType();

            IEnumerable<Type> projectionTypes = projectionsContainer.Items.Where(projectionType=> projectionType.GetContractId().Equals(version.ProjectionName, StringComparison.OrdinalIgnoreCase));
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

        /*        private IEnumerable<(IEvent, EventOrigin)> GetEventData(AggregateCommit commit)
                {
                    string aggregateId = Convert.ToBase64String(commit.AggregateRootId);
                    for (int pos = 0; pos < commit.Events.Count; pos++)
                    {
                        IEvent currentEvent = commit.Events[pos].Unwrap();

                        yield return (currentEvent, new EventOrigin(aggregateId, commit.Revision, pos, commit.Timestamp));
                    }
                }*/
    }
}
