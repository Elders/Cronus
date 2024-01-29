using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.MessageProcessing
{
    public class HandlerSubscriber : SubscriberBase
    {
        public HandlerSubscriber(Type handlerType, Workflow<HandleContext> handlerWorkflow, ILogger<HandlerSubscriber> logger) : base(handlerType, handlerWorkflow, logger) { }

        public override IEnumerable<Type> GetInvolvedMessageTypes()
        {
            Type baseMessageType = typeof(IMessage);

            return handlerType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericArguments().Length == 1 && (baseMessageType.IsAssignableFrom(x.GetGenericArguments()[0])))
                .Select(@interface => @interface.GetGenericArguments()[0])
                .Distinct();
        }
    }
}
