using System;
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
                    var interfaces = fpEventHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericMarkupInterface);

                    foreach (var @interface in interfaces)
                    {
                        Type eventType = @interface.GetGenericArguments().FirstOrDefault();

                        bus.RegisterEventHandler(eventType, fpEventHandlerType, eventHandlerFactory);
                    }
                }
            }
        }

        /// <summary>
        /// Registers all event handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static void RegisterAllEventHandlersInAssembly(this IEventBus bus, params Assembly[] asembliesContainingEventHandlers)
        {
            RegisterAllEventHandlersInAssembly(bus, (x) => (IEventHandler)Activator.CreateInstance(x), asembliesContainingEventHandlers);
        }
    }
}