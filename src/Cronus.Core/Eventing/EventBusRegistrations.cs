using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cronus.Core.Eventing
{
    public static class EventBusRegistrations
    {
        /// <summary>
        /// Registers all event handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static void RegisterAllEventHandlersInAssembly(this IEventBus bus, Func<Type, IEventHandler> eventHandlerFactory, params Assembly[] asembliesContainingEventHandlers)
        {
            foreach (Assembly assembly in asembliesContainingEventHandlers)
            {
                var eventHandlerTypes = assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IEventHandler)));
                Type genericMarkupInterface = typeof(IEventHandler<>);
                foreach (var type in eventHandlerTypes)
                {
                    Type fpEventHandlerType = type;
                    Register(bus, fpEventHandlerType, genericMarkupInterface, eventHandlerFactory);
                }
            }
        }

        static void Register(this IEventBus bus, Type eventHandlerType, Type genericMarkupInterface, Func<Type, IEventHandler> eventHandlerFactory)
        {
            var interfaces = eventHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericMarkupInterface);

            foreach (var @interface in interfaces)
            {
                Type eventType = @interface.GetGenericArguments().FirstOrDefault();

                bus.RegisterEventHandler(eventType, eventHandlerType, eventHandlerFactory);
            }
        }

        /// <summary>
        /// Registers all event handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static void RegisterAllEventHandlersInAssembly(this IEventBus bus, params Assembly[] asembliesContainingEventHandlers)
        {
            foreach (Assembly assembly in asembliesContainingEventHandlers)
            {
                var eventHandlerTypes = assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IEventHandler)));
                Type genericMarkupInterface = typeof(IEventHandler<>);
                foreach (var type in eventHandlerTypes)
                {
                    Type fpEventHandlerType = type;
                    FastActivator.WarmInstanceConstructor(fpEventHandlerType);
                    Register(bus, fpEventHandlerType, genericMarkupInterface, (x) => (IEventHandler)FastActivator.CreateInstance(x, 5));
                }
            }
        }
    }


}