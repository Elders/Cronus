using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.MessageProcessingMiddleware;
using Elders.Cronus.Middleware;
using static Elders.Cronus.MessageProcessingMiddleware.MessageHandlerMiddleware;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IMessageProcessorSettings { }

    public interface ISubscrptionMiddlewareSettings<out TContract> : ISettingsBuilder where TContract : IMessage
    {
        List<Type> HandlerRegistrations { get; set; }

        Func<Type, object> HandlerFactory { get; set; }

        Func<Middleware<HandleContext>, Middleware<HandleContext>> HandleMiddleware { get; set; }
    }
}
