﻿using System;
using System.Linq;
using System.Reflection;

namespace Elders.Cronus.Pipeline.Config
{
    public static class EndpointConsumerRegistrations
    {
        public static T RegisterHandlersInAssembly<T>(this T self, Assembly[] messageHandlers, Func<Type, object> messageHandlerFactory) where T : ISubscrptionMiddlewareSettings
        {
            var handlerTypes = messageHandlers.SelectMany(x => x.GetExportedTypes());
            self.HandlerRegistrations = handlerTypes;
            self.HandlerFactory = messageHandlerFactory;
            return self;
        }

        public static T RegisterHandlersInAssembly<T>(this T self, Assembly[] messageHandlers) where T : ISubscrptionMiddlewareSettings
        {
            return RegisterHandlersInAssembly(self, messageHandlers, t => FastActivator.CreateInstance(t));
        }
    }
}
