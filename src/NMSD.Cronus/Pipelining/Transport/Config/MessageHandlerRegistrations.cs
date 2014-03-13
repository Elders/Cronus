using System;
using System.Linq;
using System.Reflection;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging.MessageHandleScope;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public static class MessageHandlerRegistrations
    {
        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static void RegisterAllHandlersInAssembly(this IEndpointConsumerSetting consumer, Assembly assemblyContainingMessageHandlers)
        {
            Register(consumer, assemblyContainingMessageHandlers, (x, context) => FastActivator.CreateInstance(x), (messageHandlerType) => FastActivator.WarmInstanceConstructor(messageHandlerType));
        }

        public static void RegisterAllHandlersInAssembly(this IEndpointConsumerSetting consumer, Type[] messageHandlers)
        {
            Register(consumer, messageHandlers, (x, context) => FastActivator.CreateInstance(x), (messageHandlerType) => FastActivator.WarmInstanceConstructor(messageHandlerType));
        }

        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static void RegisterAllHandlersInAssembly(this IEndpointConsumerSetting consumer, Assembly assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory)
        {
            Register(consumer, assemblyContainingMessageHandlers, messageHandlerFactory, (eventHandlerType) => { });
        }

        public static void RegisterAllHandlersInAssembly(this IEndpointConsumerSetting consumer, Type[] messageHandlers, Func<Type, Context, object> messageHandlerFactory)
        {
            Register(consumer, messageHandlers, messageHandlerFactory, (eventHandlerType) => { });
        }

        static void Register(this IEndpointConsumerSetting consumer, Assembly assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory, Action<Type> doBeforeRegister)
        {
            Type genericMarkupInterface = typeof(IMessageHandler<>);

            var messageHandlerTypes = assemblyContainingMessageHandlers.GetTypes().Where(x => x.IsClass && x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericMarkupInterface));

            Register(consumer, messageHandlerTypes.ToArray(), messageHandlerFactory, doBeforeRegister);
        }

        static void Register(this IEndpointConsumerSetting consumer, Type[] messageHandlers, Func<Type, Context, object> messageHandlerFactory, Action<Type> doBeforeRegister)
        {
            Type genericMarkupInterface = typeof(IMessageHandler<>);

            var messageHandlerTypes = messageHandlers.Where(x => x.IsClass && x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericMarkupInterface));

            foreach (var messageHandlerType in messageHandlerTypes)
            {
                var fpMessageHandlerType = messageHandlerType;
                doBeforeRegister(fpMessageHandlerType);
                var interfaces = fpMessageHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericMarkupInterface);

                foreach (var @interface in interfaces)
                {
                    Type eventType = @interface.GetGenericArguments().FirstOrDefault();
                    if (eventType != null)
                        consumer.AddRegistration(eventType, fpMessageHandlerType, messageHandlerFactory);
                }
            }
        }
    }
}
