using System;
using System.Linq;
using System.Reflection;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline.Config
{
    public static class EndpointConsumerRegistrations
    {
        public static T RegisterHandlersInAssembly<T>(this T self, Assembly[] messageHandlers, Func<Type, object> messageHandlerFactory) where T : ISubscrptionMiddlewareSettings<IMessage>
        {
            var handlerTypes = messageHandlers.SelectMany(x => x.GetExportedTypes()).ToList();
            self.HandlerRegistrations = handlerTypes;
            self.HandlerFactory = messageHandlerFactory;
            return self;
        }
    }
}
