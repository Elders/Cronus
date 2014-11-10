using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.UnitOfWork;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IMessageProcessorSettings
    {

    }

    public interface IHaveUnitOfWorkFactory
    {
        Lazy<UnitOfWorkFactory> UnitOfWorkFactory { get; set; }
    }

    public interface IMessageProcessorSettings<out TContract> : ISettingsBuilder where TContract : IMessage
    {
        Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>> HandlerRegistrations { get; set; }
    }

    public class InternalApplicationServiceFactory : IMessageHandlerFactory
    {
        private readonly IMessageHandlerFactory externalFactory;

        public InternalApplicationServiceFactory(IMessageHandlerFactory externalFactory)
        {
            this.externalFactory = externalFactory;
        }

        public object CreateHandler(Type t, Context context)
        {
            object instance = externalFactory == null
                ? FastActivator.CreateInstance(t)
                : externalFactory.CreateHandler(t, context);

            instance.AssignPropertySafely<IAggregateRootApplicationService>(x => x.Repository = context.BatchContext.Get<Lazy<IAggregateRepository>>().Value);
            return instance;
        }
    }
    public interface IMessageHandlerFactory
    {
        object CreateHandler(Type t, Context ctx);
    }

    public static class EndpointConsumerRegistrations
    {
        public static T RegisterAllHandlersInAssembly<T>(this T self, Type[] messageHandlers, Func<Type, Context, object> messageHandlerFactory) where T : IMessageProcessorSettings<IMessage>
        {
            Register(self, messageHandlers, messageHandlerFactory, (eventHandlerType) => { });
            return self;
        }

        public static T RegisterAllHandlersInAssembly<T>(this T self, Assembly[] messageHandlers, Func<Type, Context, object> messageHandlerFactory) where T : IMessageProcessorSettings<IMessage>
        {
            Register(self, messageHandlers, messageHandlerFactory, (eventHandlerType) => { });
            return self;
        }

        /// <summary>
        /// Registers all command handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static T RegisterAllHandlersInAssembly<T>(this T self, Type assemblyContainingMessageHandlers, IMessageHandlerFactory messageHandlerFactory = null) where T : IMessageProcessorSettings<IMessage>
        {
            var factory = new InternalApplicationServiceFactory(messageHandlerFactory);
            Register(self, assemblyContainingMessageHandlers.Assembly, factory.CreateHandler, (eventHandlerType) => { });
            return self;
        }

        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static T RegisterAllHandlersInAssembly<T>(this T self, Assembly assemblyContainingMessageHandlers) where T : IMessageProcessorSettings<IMessage>
        {
            Register(self, assemblyContainingMessageHandlers, (x, context) => FastActivator.CreateInstance(x), (messageHandlerType) => FastActivator.WarmInstanceConstructor(messageHandlerType));
            return self;
        }

        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static T RegisterAllHandlersInAssembly<T>(this T self, Assembly assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory) where T : IMessageProcessorSettings<IMessage>
        {
            Register(self, assemblyContainingMessageHandlers, messageHandlerFactory, (eventHandlerType) => { });
            return self;
        }

        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static T RegisterAllHandlersInAssembly<T>(this T self, Type assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory) where T : IMessageProcessorSettings<IMessage>
        {
            Register(self, assemblyContainingMessageHandlers.Assembly, messageHandlerFactory, (eventHandlerType) => { });
            return self;
        }

        public static T RegisterAllHandlersInAssembly<T>(this T self, Type[] messageHandlers) where T : IMessageProcessorSettings<IMessage>
        {
            Register(self, messageHandlers, (x, context) => FastActivator.CreateInstance(x), (messageHandlerType) => FastActivator.WarmInstanceConstructor(messageHandlerType));

            return self;
        }

        static T Register<T>(this T self, Assembly assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory, Action<Type> doBeforeRegister) where T : IMessageProcessorSettings<IMessage>
        {
            var types = assemblyContainingMessageHandlers.GetTypes().ToArray();
            Register(self, types, messageHandlerFactory, doBeforeRegister);

            return self;
        }

        static T Register<T>(this T self, Assembly[] assemblyContainingMessageHandlers, Func<Type, Context, object> messageHandlerFactory, Action<Type> doBeforeRegister) where T : IMessageProcessorSettings<IMessage>
        {
            var types = assemblyContainingMessageHandlers.SelectMany(x => x.GetTypes()).ToArray();
            Register(self, types, messageHandlerFactory, doBeforeRegister);

            return self;
        }

        static T Register<T>(this T self, Type[] messageHandlers, Func<Type, Context, object> messageHandlerFactory, Action<Type> doBeforeRegister) where T : IMessageProcessorSettings<IMessage>
        {
            Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>> registrations = new Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>>();

            var contractType = self.GetType().GetInterfaces().Where(x => x.GetGenericTypeDefinition() == typeof(IMessageProcessorSettings<>)).Single().GetGenericArguments().Single();

            var contractHandlerMethodType = typeof(IEvent).IsAssignableFrom(contractType) ? typeof(IEventHandler<>) : typeof(ICommandHandler<>);

            var messageHandlerTypes = messageHandlers.Where(x => x.IsClass && x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == contractHandlerMethodType));

            foreach (var messageHandlerType in messageHandlerTypes)
            {
                var fpMessageHandlerType = messageHandlerType;
                doBeforeRegister(fpMessageHandlerType);
                var interfaces = fpMessageHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == contractHandlerMethodType);

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