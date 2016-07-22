using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline.Config
{
    public static class EndpointConsumerRegistrations
    {
        //public static T RegisterAllHandlersInAssembly<T>(this T self, Type[] messageHandlers, Func<Type, object> messageHandlerFactory) where T : ISubscrptionMiddlewareSettings<IMessage>
        //{
        //    Register(self, messageHandlers, messageHandlerFactory, (eventHandlerType) => { });
        //    return self;
        //}

        //public static T RegisterAllHandlersInAssembly<T>(this T self, Assembly[] messageHandlers, Func<Type, object> messageHandlerFactory) where T : ISubscrptionMiddlewareSettings<IMessage>
        //{
        //    Register(self, messageHandlers, messageHandlerFactory, (eventHandlerType) => { });
        //    return self;
        //}

        ///// <summary>
        ///// Registers all command handlers from a given assembly.
        ///// </summary>
        ///// <param name="assemblyContainingMessageHandlers">Assembly containing event handlers</param>
        //public static T RegisterAllHandlersInAssembly<T>(this T self, Type assemblyContainingMessageHandlers, IMessageHandlerFactory messageHandlerFactory = null) where T : ISubscrptionMiddlewareSettings<IMessage>
        //{
        //    var factory = new InternalApplicationServiceFactory(messageHandlerFactory);
        //    Register(self, assemblyContainingMessageHandlers.Assembly, factory.CreateHandler, (eventHandlerType) => { });
        //    return self;
        //}

        ///// <summary>
        ///// Registers all message handlers from a given assembly.
        ///// </summary>
        ///// <param name="assemblyContainingMessageHandlers">Assembly containing event handlers</param>
        //public static T RegisterAllHandlersInAssembly<T>(this T self, Assembly assemblyContainingMessageHandlers) where T : ISubscrptionMiddlewareSettings<IMessage>
        //{
        //    Register(self, assemblyContainingMessageHandlers, (x) => FastActivator.CreateInstance(x), (messageHandlerType) => FastActivator.WarmInstanceConstructor(messageHandlerType));
        //    return self;
        //}

        ///// <summary>
        ///// Registers all message handlers from a given assembly.
        ///// </summary>
        ///// <param name="assemblyContainingMessageHandlers">Assembly containing event handlers</param>
        //public static T RegisterAllHandlersInAssembly<T>(this T self, Assembly assemblyContainingMessageHandlers, Func<Type, object> messageHandlerFactory) where T : ISubscrptionMiddlewareSettings<IMessage>
        //{
        //    Register(self, assemblyContainingMessageHandlers, messageHandlerFactory, (eventHandlerType) => { });
        //    return self;
        //}

        ///// <summary>
        ///// Registers all message handlers from a given assembly.
        ///// </summary>
        ///// <param name="assemblyContainingMessageHandlers">Assembly containing event handlers</param>
        //public static T RegisterAllHandlersInAssembly<T>(this T self, Type assemblyContainingMessageHandlers, Func<Type, object> messageHandlerFactory) where T : ISubscrptionMiddlewareSettings<IMessage>
        //{
        //    Register(self, assemblyContainingMessageHandlers.Assembly, messageHandlerFactory, (eventHandlerType) => { });
        //    return self;
        //}

        //public static T RegisterAllHandlersInAssembly<T>(this T self, Type[] messageHandlers) where T : ISubscrptionMiddlewareSettings<IMessage>
        //{
        //    Register(self, messageHandlers, (x) => FastActivator.CreateInstance(x), (messageHandlerType) => FastActivator.WarmInstanceConstructor(messageHandlerType));

        //    return self;
        //}

        //static T Register<T>(this T self, Assembly assemblyContainingMessageHandlers, Func<Type, object> messageHandlerFactory, Action<Type> doBeforeRegister) where T : ISubscrptionMiddlewareSettings<IMessage>
        //{
        //    var types = assemblyContainingMessageHandlers.GetTypes().ToArray();
        //    Register(self, types, messageHandlerFactory, doBeforeRegister);

        //    return self;
        //}

        public static T RegisterAllEventHandlersInAssembly<T>(this T self, Assembly[] messageHandlers, Func<Type, object> messageHandlerFactory) where T : ISubscrptionMiddlewareSettings<IEvent>
        {
            var eventHandlers = messageHandlers.SelectMany(x => x.GetTypes()).Where(x => x.GetInterfaces().Any(y => y.IsGenericTypeDefinition && y.GetGenericTypeDefinition() == typeof(IEventHandler<>))).ToList();
            self.HandlerRegistrations = eventHandlers;
            self.HandlerFactory = messageHandlerFactory;
            return self;
        }

        public static T RegisterAllCommandHandlersInAssembly<T>(this T self, Assembly[] messageHandlers, Func<Type, object> messageHandlerFactory) where T : ISubscrptionMiddlewareSettings<ICommand>
        {
            var commandHandlers = messageHandlers.SelectMany(x => x.GetTypes()).Where(x => x.GetInterfaces().Any(y => y.IsGenericTypeDefinition && y.GetGenericTypeDefinition() == typeof(ICommandHandler<>))).ToList();
            self.HandlerRegistrations = commandHandlers;
            self.HandlerFactory = messageHandlerFactory;
            return self;
        }


        //static T Register<T>(this T self, Assembly[] assemblyContainingMessageHandlers, Func<Type, object> messageHandlerFactory, Action<Type> doBeforeRegister) where T : ISubscrptionMiddlewareSettings<IMessage>
        //{
        //    var types = assemblyContainingMessageHandlers.SelectMany(x => x.GetTypes()).ToArray();
        //    Register(self, types, messageHandlerFactory, doBeforeRegister);

        //    return self;
        //}

        //static T Register<T>(this T self, Type[] messageHandlers, Func<Type, object> messageHandlerFactory, Action<Type> doBeforeRegister) where T : ISubscrptionMiddlewareSettings<IMessage>
        //{
        //    Dictionary<Type, List<Type>> registrations = new Dictionary<Type, List<Type>>();

        //    var contractType = self.GetType().GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISubscrptionMiddlewareSettings<>)).Single().GetGenericArguments().Single();

        //    var contractHandlerMethodType = typeof(IEvent).IsAssignableFrom(contractType) ? typeof(IEventHandler<>) : typeof(ICommandHandler<>);

        //    var messageHandlerTypes = messageHandlers.Where(x => x.IsClass && x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == contractHandlerMethodType));

        //    foreach (var messageHandlerType in messageHandlerTypes)
        //    {
        //        var fpMessageHandlerType = messageHandlerType;
        //        doBeforeRegister(fpMessageHandlerType);
        //        var interfaces = fpMessageHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == contractHandlerMethodType);

        //        foreach (var @interface in interfaces)
        //        {
        //            Type eventType = @interface.GetGenericArguments().FirstOrDefault();
        //            if (eventType != null)
        //            {
        //                if (!registrations.ContainsKey(eventType))
        //                {
        //                    registrations.Add(eventType, new List<Type>());
        //                }
        //                registrations[eventType].Add(fpMessageHandlerType);
        //            }
        //        }
        //    }

        //    self.HandlerRegistrations = registrations;
        //    self.HandlerFactory = messageHandlerFactory;
        //    return self;
        //}

    }
}
