using System;
using System.Linq;
using System.Reflection;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Publishing;

namespace Cronus.Core.Eventing
{
    public static class PublisherRegistrations
    {
        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static void RegisterAllCommandHandlersInAssembly(this InMemoryCommandBus bus, params Assembly[] asembliesContainingCommandHandlers)
        {
            Register(bus, (commandHandlerType) => FastActivator.WarmInstanceConstructor(commandHandlerType), (x) => (ICommandHandler)FastActivator.CreateInstance(x), asembliesContainingCommandHandlers);
        }

        /// <summary>
        /// Registers all message handlers from a given assembly.
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        public static void RegisterAllCommandHandlersInAssembly(this InMemoryCommandBus bus, Func<Type, ICommandHandler> commandHandlerFactory, params Assembly[] asembliesContainingCommandHandlers)
        {
            Register(bus, (eventHandlerType) => { }, commandHandlerFactory, asembliesContainingCommandHandlers);
        }

        static void Register(this InMemoryCommandBus bus, Action<Type> doBeforeRegister, Func<Type, ICommandHandler> commandHandlerFactory, params Assembly[] asembliesContainingCommandHandlers)
        {
            foreach (Assembly assembly in asembliesContainingCommandHandlers)
            {
                var commandHandlerTypes = assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(ICommandHandler)));
                Type genericMarkupInterface = typeof(ICommandHandler<>);
                foreach (var commandHandlerType in commandHandlerTypes)
                {
                    var fpCommandHandlerType = commandHandlerType;
                    doBeforeRegister(fpCommandHandlerType);
                    var interfaces = fpCommandHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericMarkupInterface);

                    foreach (var @interface in interfaces)
                    {
                        Type eventType = @interface.GetGenericArguments().FirstOrDefault();
                        bus.RegisterHandler(eventType, fpCommandHandlerType, commandHandlerFactory);
                    }
                }
            }
        }
    }
}