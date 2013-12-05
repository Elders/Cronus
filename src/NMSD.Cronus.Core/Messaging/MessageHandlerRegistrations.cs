using System;
using System.Linq;
using System.Reflection;
using Cronus.Core;
using NMSD.Cronus.Core.Commanding;

namespace NMSD.Cronus.Core.Messaging
{
    public static class MessageHandlerRegistrations
    {
        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static void RegisterAllHandlersInAssembly<THandler>(this IConsumer<THandler> bus, params Assembly[] asembliesContainingMessageHandlers)
        {
            Register(bus, (messageHandlerType) => FastActivator.WarmInstanceConstructor(messageHandlerType), (x) => (THandler)FastActivator.CreateInstance(x), asembliesContainingMessageHandlers);
        }

        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static void RegisterAllHandlersInAssembly<THandler>(this IConsumer<THandler> bus, Func<Type, THandler> commandHandlerFactory, params Assembly[] asembliesContainingMessageHandlers)
        {
            Register(bus, (eventHandlerType) => { }, commandHandlerFactory, asembliesContainingMessageHandlers);
        }

        static void Register<THandler>(this IConsumer<THandler> bus, Action<Type> doBeforeRegister, Func<Type, THandler> messageHandlerFactory, params Assembly[] asembliesContainingMessageHandlers)
        {
            foreach (Assembly assembly in asembliesContainingMessageHandlers)
            {
                var commandHandlerTypes = assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(THandler)) && x.IsClass);
                Type genericMarkupInterface = typeof(IMessageHandler<>);
                foreach (var messageHandlerType in commandHandlerTypes)
                {
                    var fpMessageHandlerType = messageHandlerType;
                    doBeforeRegister(fpMessageHandlerType);
                    var interfaces = fpMessageHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericMarkupInterface);

                    foreach (var @interface in interfaces)
                    {
                        Type eventType = @interface.GetGenericArguments().FirstOrDefault();
                        if (eventType != null)
                            bus.RegisterHandler(eventType, fpMessageHandlerType, messageHandlerFactory);
                    }
                }
            }
        }
    }
}