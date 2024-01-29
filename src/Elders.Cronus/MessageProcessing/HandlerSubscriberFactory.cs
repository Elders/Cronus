using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;
using System;

namespace Elders.Cronus.MessageProcessing
{
    public class HandlerSubscriberFactory<T> : ISubscriberFactory<T>
    {
        private readonly Workflow<HandleContext> workflow;
        private readonly ILogger<HandlerSubscriber> logger;

        public HandlerSubscriberFactory(ISubscriberWorkflowFactory<T> subscriberWorkflow, ILogger<HandlerSubscriber> logger)
        {
            workflow = subscriberWorkflow.GetWorkflow() as Workflow<HandleContext>;
            this.logger = logger;
        }

        public ISubscriber Create(Type handlerType)
        {
            return new HandlerSubscriber(handlerType, workflow, logger);
        }
    }
}
