using System;
using System.Collections.Generic;
using System.Linq;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging.MessageHandleScope;

namespace NMSD.Cronus
{
    public class MessageHandlerCollection<TMessage>
        where TMessage : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MessageHandlerCollection<TMessage>));

        public ScopeFactory ScopeFactory { get; set; }

        //  Each contract/message can have several handler types/classes
        protected Dictionary<Type, HashSet<Type>> registeredHandlers = new Dictionary<Type, HashSet<Type>>();

        //  Each handler can handle different contracts/messages where the actual handler body is LateBoundVoidMethod
        protected Dictionary<Type, Dictionary<Type, LateBoundVoidMethod>> handlerCallbacks = new Dictionary<Type, Dictionary<Type, LateBoundVoidMethod>>();

        //  Each contract/message is bound to an actual handle implementation and the reason we do it like this is because of the handlerFactory
        private Dictionary<Type, List<Func<TMessage, bool>>> handlers = new Dictionary<Type, List<Func<TMessage, bool>>>();

        public IEnumerable<Type> GetRegisteredHandlers()
        {
            return registeredHandlers.Keys.AsEnumerable();
        }

        public virtual void RegisterHandler(Type messageType, Type messageHandlerType, Func<Type, Context, object> handlerFactory)
        {
            if (!registeredHandlers.ContainsKey(messageHandlerType))
                registeredHandlers.Add(messageHandlerType, new HashSet<Type>());

            registeredHandlers[messageHandlerType].Add(messageType);

            if (!handlers.ContainsKey(messageType))
                handlers[messageType] = new List<Func<TMessage, bool>>();

            handlers[messageType].Add(x => Handle(x, messageHandlerType, handlerFactory));
            if (!handlerCallbacks.ContainsKey(messageHandlerType))
            {
                var callbacks = new Dictionary<Type, LateBoundVoidMethod>();
                var handleMethodInfos = messageHandlerType.GetMethods().Where(x => x.Name == "Handle");
                foreach (var mi in handleMethodInfos)
                {
                    var callback = DelegateFactory.Create(mi);
                    callbacks.Add(mi.GetParameters().First().ParameterType, callback);
                }
                handlerCallbacks.Add(messageHandlerType, callbacks);
            }
        }

        public bool Handle(TMessage message)
        {
            return ScopeFactory.UseMessageScope(scope =>
            {
                List<Func<TMessage, bool>> availableHandlers;
                if (handlers.TryGetValue(message.GetType(), out availableHandlers))
                {
                    foreach (var handleMethod in availableHandlers)
                    {
                        handleMethod(message);
                    }
                }
                //  TODO: If one handle crashes then do something like propagading the error to the caller?!?
                return true;
            });
        }

        protected bool Handle(TMessage message, Type eventHandlerType, Func<Type, Context, object> handlerFactory)
        {
            return ScopeFactory.UseHandlerScope(scope =>
            {
                var handler = handlerFactory(eventHandlerType, ScopeFactory.CurrentContext);
                handlerCallbacks[eventHandlerType][message.GetType()](handler, new object[] { message });
                if (log.IsInfoEnabled)
                    log.Info("HANDLE => " + message.ToString());

                return true;
            });
        }
    }

}