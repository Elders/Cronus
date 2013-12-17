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
        public static void RegisterAllHandlersInAssembly<THandler>(this IConsumer<THandler> bus, Assembly assemblyContainingMessageHandlers)
        {
            Register(bus, assemblyContainingMessageHandlers, (x) => (THandler)FastActivator.CreateInstance(x), (messageHandlerType) => FastActivator.WarmInstanceConstructor(messageHandlerType));
        }

        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static void RegisterAllHandlersInAssembly<THandler>(this IConsumer<THandler> bus, Assembly assemblyContainingMessageHandlers, Func<Type, THandler> messageHandlerFactory)
        {
            Register(bus, assemblyContainingMessageHandlers, messageHandlerFactory, (eventHandlerType) => { });
        }

        static void Register<THandler>(this IConsumer<THandler> bus, Assembly assemblyContainingMessageHandlers, Func<Type, THandler> messageHandlerFactory, Action<Type> doBeforeRegister)
        {
            var messageHandlerTypes = assemblyContainingMessageHandlers.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(THandler)) && x.IsClass);
            Type genericMarkupInterface = typeof(IMessageHandler<>);
            foreach (var messageHandlerType in messageHandlerTypes)
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