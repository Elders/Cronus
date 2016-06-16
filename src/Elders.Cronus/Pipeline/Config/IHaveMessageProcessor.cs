using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Middleware;
using static Elders.Cronus.MessageProcessingMiddleware.MessageHandlerMiddleware;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IMessageProcessorSettings { }

    public interface IMessageProcessorSettings<out TContract> : ISettingsBuilder where TContract : IMessage
    {
        Dictionary<Type, List<Type>> HandlerRegistrations { get; set; }

        Func<Type, object> HandlerFactory { get; set; }

        string MessageProcessorName { get; set; }

        Middleware<HandleContext> ActualHandle { get; set; }
    }
}
