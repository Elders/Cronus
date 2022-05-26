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
    public class ProjectionIndex : IEventStoreIndex, ICronusEventStoreIndex
    {
        private static Type baseMessageType = typeof(IMessage);

        private readonly ILogger<ProjectionIndex> logger;
        private readonly TypeContainer<IProjection> projectionsContainer;
        private readonly IProjectionWriter projection;

        public ProjectionIndex(TypeContainer<IProjection> projectionsContainer, IProjectionWriter projection, ILogger<ProjectionIndex> logger)
        {
            this.projectionsContainer = projectionsContainer;
            this.projection = projection;
            this.logger = logger;
        }

        public async Task IndexAsync(CronusMessage message)
        {
            IEnumerable<Type> projectionTypes = projectionsContainer.Items;
            if (message.IsRepublished)
                projectionTypes = message.RecipientHandlers.Intersect(projectionsContainer.Items.Select(t => t.GetContractId())).Select(dc => dc.GetTypeByContract());

            List<Task> projectionSaveTasks = new List<Task>();
            Type messagePayloadType = message.Payload.GetType();
            foreach (var projectionType in projectionTypes)
            {
                bool isInterested = projectionType.GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericArguments().Length == 1 && (baseMessageType.IsAssignableFrom(x.GetGenericArguments()[0])))
                    .Where(@interface => @interface.GetGenericArguments()[0].IsAssignableFrom(messagePayloadType))
                    .Any();

                if (isInterested)
                {
                    projectionSaveTasks.Add(projection.SaveAsync(projectionType, message));
                }
            }

            await Task.WhenAll(projectionSaveTasks).ConfigureAwait(false);
        }

        public async Task IndexAsync(AggregateCommit aggregateCommit, ProjectionVersion version)
        {
            IEnumerable<(IEvent, EventOrigin)> eventDataList = GetEventData(aggregateCommit);

            foreach (var eventData in eventDataList)
            {
                List<Task> projectionSaveTasks = new List<Task>();
                Type messagePayloadType = eventData.Item1.GetType();
                foreach (var projectionType in projectionsContainer.Items)
                {
                    if (projectionType.GetContractId().Equals(version.ProjectionName, StringComparison.OrdinalIgnoreCase) == false)
                        continue;

                    bool isInterested = projectionType.GetInterfaces()
                        .Where(@interface => @interface.IsGenericType && @interface.GetGenericArguments().Length == 1 && (baseMessageType.IsAssignableFrom(@interface.GetGenericArguments()[0])))
                        .Where(@interface => @interface.GetGenericArguments()[0].IsAssignableFrom(messagePayloadType))
                        .Any();

                    if (isInterested)
                    {
                        projectionSaveTasks.Add(projection.SaveAsync(projectionType, eventData.Item1, eventData.Item2, version));
                    }
                }

                await Task.WhenAll(projectionSaveTasks).ConfigureAwait(false);
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
