using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.UnitOfWork;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IHaveMessageProcessor<TContract> where TContract : IMessage
    {
        Lazy<IMessageProcessor<TContract>> MessageHandlerProcessor { get; set; }
    }

    public interface IMessageProcessorSettings
    {

    }

    public interface IHaveUnitOfWorkFactory
    {
        Lazy<UnitOfWorkFactory> UnitOfWorkFactory { get; set; }
    }

    public interface IMessageProcessorWithSafeBatchSettings<out TContract> where TContract : IMessage
    {
        Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>> HandlerRegistrations { get; set; }
    }

    public static class EndpointConsumerRegistrations
    {
        public static T RegisterAllHandlersInAssembly<T>(this T self, Type[] messageHandlers, Func<Type, Context, object> messageHandlerFactory) where T : IMessageProcessorWithSafeBatchSettings<IMessage>
        {
            Register(self, messageHandlers, messageHandlerFactory, (eventHandlerType) => { });
            return self;
        }

        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static T RegisterAllHandlersInAssembly<T>(this T self, Assembly assemblyContainingMessageHandlers) where T : IMessageProcessorWithSafeBatchSettings<IMessage>
        {
            Register(self, assemblyContainingMessageHandlers, (x, context) => FastActivator.CreateInstance(x), (messageHandlerType) => FastActivator.WarmInstanceConstructor(messageHandlerType));
            return self;
        }

        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static T RegisterAllHandlersInAssembly<T>(this T self, Assembly assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory) where T : IMessageProcessorWithSafeBatchSettings<IMessage>
        {
            Register(self, assemblyContainingMessageHandlers, messageHandlerFactory, (eventHandlerType) => { });
            return self;
        }

        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static T RegisterAllHandlersInAssembly<T>(this T self, Type assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory) where T : IMessageProcessorWithSafeBatchSettings<IMessage>
        {
            Register(self, assemblyContainingMessageHandlers.Assembly, messageHandlerFactory, (eventHandlerType) => { });
            return self;
        }

        public static T RegisterAllHandlersInAssembly<T>(this T self, Type[] messageHandlers) where T : IMessageProcessorWithSafeBatchSettings<IMessage>
        {
            Register(self, messageHandlers, (x, context) => FastActivator.CreateInstance(x), (messageHandlerType) => FastActivator.WarmInstanceConstructor(messageHandlerType));

            return self;
        }

        static T Register<T>(this T self, Assembly assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory, Action<Type> doBeforeRegister) where T : IMessageProcessorWithSafeBatchSettings<IMessage>
        {
            Type genericMarkupInterface = typeof(IMessageHandler<>);

            var messageHandlerTypes = assemblyContainingMessageHandlers.GetTypes().Where(x => x.IsClass && x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericMarkupInterface));

            Register(self, messageHandlerTypes.ToArray(), messageHandlerFactory, doBeforeRegister);

            return self;
        }

        static T Register<T>(this T self, Type[] messageHandlers, Func<Type, Context, object> messageHandlerFactory, Action<Type> doBeforeRegister) where T : IMessageProcessorWithSafeBatchSettings<IMessage>
        {
            Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>> registrations = new Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>>();

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
                    {
                        if (!registrations.ContainsKey(eventType))
                        {
                            registrations.Add(eventType, new List<Tuple<Type, Func<Type, Context, object>>>());
                        }
                        registrations[eventType].Add(new Tuple<Type, Func<Type, Context, object>>(fpMessageHandlerType, messageHandlerFactory));
                    }
                }
            }

            self.HandlerRegistrations = registrations;
            return self;
        }
    }
}