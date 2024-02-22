using Elders.Cronus.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index;

[DataContract(Name = "37336a18-573a-4e9e-b4a2-085033b74353")]
public class ProjectionIndex : ICronusEventStoreIndex
{
    private readonly TypeContainer<IProjection> projectionsContainer;
    private readonly IProjectionWriter projection;

    public ProjectionIndex(TypeContainer<IProjection> projectionsContainer, IProjectionWriter projection)
    {
        this.projectionsContainer = projectionsContainer;
        this.projection = projection;
    }

    public async Task IndexAsync(CronusMessage message)
    {
        IEnumerable<Type> projectionTypes = projectionsContainer.Items;
        if (message.IsRepublished)
            projectionTypes = message.RecipientHandlers.Intersect(projectionsContainer.Items.Select(t => t.GetContractId())).Select(dc => dc.GetTypeByContract());

        List<Task> tasks = new List<Task>();
        Type messagePayloadType = message.Payload.GetType();
        foreach (var projectionType in projectionTypes)
        {
            bool isInterested = projectionType.GetInterfaces()
                .Where(@interface => IsInterested(@interface, messagePayloadType))
                .Any();

            if (isInterested)
            {
                var task = projection.SaveAsync(projectionType, message.Payload as IEvent);
                tasks.Add(task);
            }
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static bool IsInterested(Type handlerInterfaces, Type messagePayloadType)
    {
        var genericArguments = handlerInterfaces.GetGenericArguments();

        return handlerInterfaces.IsGenericType && genericArguments.Length == 1 && messagePayloadType.IsAssignableFrom(genericArguments[0]);
    }
}
