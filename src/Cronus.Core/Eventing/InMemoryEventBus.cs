using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cronus.Core.Eventing
{
    /// <summary>
    /// Represents an in memory event messaging destribution
    /// </summary>
    public class InMemoryEventBus:IEventBus
    {
        Dictionary<Type, List<Action<IEvent>>> handlers = new Dictionary<Type, List<Action<IEvent>>>();

        Func<Type, IEventHandler> eventHandlerFactory;

        /// <summary>
        /// Creates an instance of InMemoryEventBus
        /// </summary>
        /// <param name="eventHandlerFactoryMethod">Factory method for creating a Event Handlers Instances by a given type which implements IEventHandler(probably IoC container will resolve them)</param>
        /// <param name="assembliesContainingEventHandlers">Assembly containing event handlers</param>
        public InMemoryEventBus(Func<Type,IEventHandler> eventHandlerFactoryMethod,params Assembly[] assembliesContainingEventHandlers)
        {
            this.eventHandlerFactory = eventHandlerFactoryMethod;
            foreach (var asembbly in assembliesContainingEventHandlers)
            {
                RegisterAllEventHandlersInAssembly(asembbly);
            }
        }

        /// <summary>
        /// Registers all event handlers from a given assembly to the <typeparamref name="InMemoryEventBus"/>
        /// </summary>
        /// <param name="asemblyContainingEventHandlers">Assembly containing event handlers</param>
        private void RegisterAllEventHandlersInAssembly(Assembly asemblyContainingEventHandlers)
        {
           var eventHandlerTypes = asemblyContainingEventHandlers.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IEventHandler)));
           Type genericMarkupInterface = typeof(IEventHandler<>);
           foreach (var type in eventHandlerTypes)
           {
               Type fpEventHandlerType = type;
               var interfaces = fpEventHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericMarkupInterface);

               foreach (var @interface in interfaces)
               { 
                   Type eventType=@interface.GetGenericArguments().FirstOrDefault();

                   if(!handlers.ContainsKey(eventType))
                   {
                       handlers[eventType] = new List<Action<IEvent>>();
                   }
                   var handleMethod = type.GetMethods().Where(x => x.Name == "Handle" && x.GetParameters().Count() == 1 && x.GetParameters().Select(y => y.ParameterType).Contains(eventType)).SingleOrDefault(); ;
                   
                   handlers[eventType].Add(x =>
                   {
                       var handler = eventHandlerFactory(fpEventHandlerType);
                       handleMethod.Invoke(handler,new object[]{x});
                   });
               }
           }
        }

        /// <summary>
        /// Publishes the given event to all registered event handlers
        /// </summary>
        /// <param name="event">An event instance</param>
        public void Publish(IEvent @event)
        {
            foreach (var handleMethod in handlers[@event.GetType()])
            {
                handleMethod(@event);
            }
        }
    }
}
