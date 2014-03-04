using System;
using System.Linq;
using System.Reflection;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Hosting;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Messaging.MessageHandleScope;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public static class MessageHandlerRegistrations
    {
        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static void RegisterAllHandlersInAssembly<T>(this PipelineConsumerSettings<T> consumer, Assembly assemblyContainingMessageHandlers) where T : IStartableConsumer<IMessage>
        {
            Register(consumer, assemblyContainingMessageHandlers, (x, context) => FastActivator.CreateInstance(x), (messageHandlerType) => FastActivator.WarmInstanceConstructor(messageHandlerType));
        }

        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static void RegisterAllHandlersInAssembly<T>(this PipelineConsumerSettings<T> consumer, Assembly assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory) where T : IStartableConsumer<IMessage>
        {
            Register(consumer, assemblyContainingMessageHandlers, messageHandlerFactory, (eventHandlerType) => { });
        }

        static void Register<T>(this PipelineConsumerSettings<T> consumer, Assembly assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory, Action<Type> doBeforeRegister) where T : IStartableConsumer<IMessage>
        {
            Type genericMarkupInterface = typeof(IMessageHandler<>);

            var messageHandlerTypes = assemblyContainingMessageHandlers.GetTypes().Where(x => x.IsClass && x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericMarkupInterface));

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

        static bool IsHandlerClass(Type possbileHandler)
        {
            Type genericMarkupInterface = typeof(IMessageHandler<>);

            return possbileHandler.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericMarkupInterface);
        }
    }
}
