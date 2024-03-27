using System;
using System.Collections.Generic;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.Projections.Versioning;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.MessageProcessing;

public class EventStoreIndexSubscriberFactory<TIndex> : ISubscriberFactory<TIndex>
    where TIndex : IEventStoreIndex
{
    private readonly Workflow<HandleContext> workflow;
    private readonly IOptions<BoundedContext> boundedContextOptions;
    private readonly TypeContainer<IEvent> allEventTypesInTheSystem;
    private readonly TypeContainer<IPublicEvent> allPublicEventTypesInTheSystem;

    public EventStoreIndexSubscriberFactory(IOptions<BoundedContext> boundedContextOptions, ISubscriberWorkflowFactory<TIndex> subscriberWorkflow, TypeContainer<IEvent> allEventTypesInTheSystem, TypeContainer<IPublicEvent> allPublicEventTypesInTheSystem)
    {
        workflow = subscriberWorkflow.GetWorkflow() as Workflow<HandleContext>;
        this.boundedContextOptions = boundedContextOptions;
        this.allEventTypesInTheSystem = allEventTypesInTheSystem;
        this.allPublicEventTypesInTheSystem = allPublicEventTypesInTheSystem;
    }

    public ISubscriber Create(Type indexType)
    {
        string currentBoundedContext = boundedContextOptions.Value.Name;

        IEnumerable<Type> currentBoundedContextEvents = GetBoundedContextMessages(allEventTypesInTheSystem.Items, currentBoundedContext);
        IEnumerable<Type> currentBoundedContextPublicEvents = GetBoundedContextMessages(allPublicEventTypesInTheSystem.Items, currentBoundedContext);

        return new EventStoreIndexSubscriber(indexType, workflow, currentBoundedContextEvents, currentBoundedContextPublicEvents);
    }

    private IEnumerable<Type> GetBoundedContextMessages(IEnumerable<Type> messages, string boundedContext)
    {
        foreach (var msg in messages)
        {
            string msgBoundedContext = msg.GetBoundedContext(boundedContext);
            if (msgBoundedContext == boundedContext)
                yield return msg;
            else if (msgBoundedContext.Equals(boundedContext, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Please make sure the bounded context names are the same case. Bounded context from message: {msgBoundedContext}, expected bounded context: {boundedContext}");
        }
    }
}
