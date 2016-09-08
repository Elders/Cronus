using System;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.Pipeline.Config
{
    public static class MiddlewareExtensions
    {
        public static ISubscrptionMiddlewareSettings Middleware(this ISubscrptionMiddlewareSettings self, Func<Middleware<HandleContext>, Middleware<HandleContext>> middlewareConfig)
        {
            self.HandleMiddleware = middlewareConfig;
            return self;
        }
    }
}
