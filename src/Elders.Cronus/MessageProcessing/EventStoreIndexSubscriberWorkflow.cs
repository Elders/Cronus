using Elders.Cronus.EventStore.Index;
using Elders.Cronus.FaultHandling;
using Elders.Cronus.Workflow;
using System;

namespace Elders.Cronus.MessageProcessing
{
    /// <summary>
    /// Work-flow which handles all events and writes them into the index
    /// </summary>
    public class EventStoreIndexSubscriberWorkflow : ISubscriberWorkflow<IEventStoreIndex>
    {
        private readonly IServiceProvider serviceProvider;

        public EventStoreIndexSubscriberWorkflow(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IWorkflow GetWorkflow()
        {
            var messageHandleWorkflow = new MessageHandleWorkflow(new CreateScopedHandlerWorkflow());
            messageHandleWorkflow.ActualHandle.Override(new DynamicMessageIndex());
            var scopedWorkflow = new ScopedMessageWorkflow(serviceProvider, messageHandleWorkflow);
            var customWorkflow = new InMemoryRetryWorkflow<HandleContext>(scopedWorkflow);

            return customWorkflow;
        }
    }
}
