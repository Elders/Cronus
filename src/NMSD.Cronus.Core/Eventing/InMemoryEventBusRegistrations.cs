using System;
using System.Linq;
using System.Reflection;
using NMSD.Cronus.Core.Eventing;

namespace Cronus.Core.Eventing
{
    //public static class InMemoryEventBusRegistrations
    //{
    //    /// <summary>
    //    /// Registers all message handlers from a given assembly.
    //    /// </summary>
    //    /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
    //    public static void RegisterAllEventHandlersInAssembly(this InMemoryEventBus bus, params Assembly[] asembliesContainingEventHandlers)
    //    {
    //        Register(bus, (EventHandlerType) => FastActivator.WarmInstanceConstructor(EventHandlerType), (x) => (IEventHandler)FastActivator.CreateInstance(x), asembliesContainingEventHandlers);
    //    }

    //    /// <summary>
    //    /// Registers all message handlers from a given assembly.
    //    /// </summary>
    //    /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
    //    public static void RegisterAllEventHandlersInAssembly(this InMemoryEventBus bus, Func<Type, IEventHandler> EventHandlerFactory, params Assembly[] asembliesContainingEventHandlers)
    //    {
    //        Register(bus, (eventHandlerType) => { }, EventHandlerFactory, asembliesContainingEventHandlers);
    //    }

    //    static void Register(this InMemoryEventBus bus, Action<Type> doBeforeRegister, Func<Type, IEventHandler> EventHandlerFactory, params Assembly[] asembliesContainingEventHandlers)
    //    {
    //        foreach (Assembly assembly in asembliesContainingEventHandlers)
    //        {
    //            var EventHandlerTypes = assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IEventHandler)));
    //            Type genericMarkupInterface = typeof(IEventHandler<>);
    //            foreach (var EventHandlerType in EventHandlerTypes)
    //            {
    //                var fpEventHandlerType = EventHandlerType;
    //                doBeforeRegister(fpEventHandlerType);
    //                var interfaces = fpEventHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericMarkupInterface);

    //                foreach (var @interface in interfaces)
    //                {
    //                    Type eventType = @interface.GetGenericArguments().FirstOrDefault();
    //                    bus.RegisterHandler(eventType, fpEventHandlerType, EventHandlerFactory);
    //                }
    //            }
    //        }
    //    }
    //}
}