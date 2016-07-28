using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.FaultHandling;
using Elders.Cronus.IocContainer;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.Pipeline.Config
{
    public static class MiddlewareExtensions
    {
        public static ISubscrptionMiddlewareSettings<T> Middleware<T>(this ISubscrptionMiddlewareSettings<T> self, Func<Middleware<HandleContext>, Middleware<HandleContext>> middlewareConfig)
            where T : IMessage
        {
            self.HandleMiddleware = middlewareConfig;
            return self;
        }
    }
}