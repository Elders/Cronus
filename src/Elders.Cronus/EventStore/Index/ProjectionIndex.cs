using Elders.Cronus.Projections;
using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "37336a18-573a-4e9e-b4a2-085033b74353")]
    public class ProjectionIndex : IEventStoreIndex, ICronusEventStoreIndex
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

        public void Index(AggregateCommit aggregateCommit, ProjectionVersion version)
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
                        projection.Save(projectionType, eventData.Item1, eventData.Item2, version);
                    }
                }
            }
        }

        private IEnumerable<(IEvent, EventOrigin)> GetEventData(AggregateCommit commit)
        {
            string aggregateId = Convert.ToBase64String(commit.AggregateRootId);
            for (int pos = 0; pos < commit.Events.Count; pos++)
            {
                IEvent currentEvent = commit.Events[pos].Unwrap();

                yield return (currentEvent, new EventOrigin(aggregateId, commit.Revision, pos, commit.Timestamp));
            }
        }
    }
}
