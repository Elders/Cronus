using System;
using System.Collections.Generic;
using System.IO;
using RabbitMQ.Client.Exceptions;

namespace NMSD.Cronus.Core.Messaging
{
    public abstract class Publisher<TMessage, THandler> : IPublisher<TMessage>, IConsumer<THandler>
    {
        protected Dictionary<Type, List<Func<TMessage, bool>>> handlers = new Dictionary<Type, List<Func<TMessage, bool>>>();

        protected Action<TMessage, THandler, Exception> onErrorHandlingEvent = (x, y, z) => { };

        protected Action<TMessage, THandler> onEventHandled = (x, y) => { };

        protected Action<TMessage> onEventPublished = (x => { });

        protected Action<TMessage, THandler> onHandleEvent = (x, y) => { };

        protected Action<TMessage> onPublishEvent = (x => { });

        public void OnErrorHandlingEvent(Action<TMessage, THandler, Exception> action)
        {
            onErrorHandlingEvent = action;
        }

        public void OnEventHandled(Action<TMessage, THandler> action)
        {
            onEventHandled = action;
        }

        public void OnEventPublished(Action<TMessage> action)
        {
            onEventPublished = action;
        }

        public void OnHandleEvent(Action<TMessage, THandler> action)
        {
            onHandleEvent = action;
        }

        public void OnPublishEvent(Action<TMessage> action)
        {
            onPublishEvent = action;
        }

        public abstract bool Publish(TMessage message);

        public void RegisterHandler(Type eventType, Type eventHandlerType, Func<Type, THandler> handlerFactory)
        {
            if (!handlers.ContainsKey(eventType))
                handlers[eventType] = new List<Func<TMessage, bool>>();

            handlers[eventType].Add(x => Handle(x, eventHandlerType, handlerFactory));
        }

        bool Handle(TMessage message, Type eventHandlerType, Func<Type, THandler> handlerFactory)
        {
            dynamic handler = null;
            try
            {
                handler = handlerFactory(eventHandlerType);
                onHandleEvent(message, handler);
                handler.Handle((dynamic)message);
                onEventHandled(message, handler);
                return true;
            }
            catch (Exception ex)
            {
                onErrorHandlingEvent(message, handler, ex);
                return false;
            }
        }
    }

    public abstract class Publisher<TMessage> : IPublisher<TMessage>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Publisher<TMessage>));

        protected Action<TMessage> beforePublish;

        protected Action<TMessage> afterPublish;

        public void SetBeforePublishAction(Action<TMessage> action)
        {
            beforePublish = action;
        }

        public void SetAfterPublishAction(Action<TMessage> action)
        {
            afterPublish = action;
        }

        protected abstract bool PublishInternal(TMessage message);

        public bool Publish(TMessage message)
        {
            //if (beforePublish != null) beforePublish(message);
            try
            {
                PublishInternal(message);
            }
            catch (AlreadyClosedException ex)
            {
                var error = String.Format("Unable to connect to RabbitMQ broker. Consequences: Cannot publish message '{0}'", message.ToString());
                log.Error(error, ex);
                return false;
            }
            catch (IOException ex)
            {
                var error = String.Format("Unable to connect to RabbitMQ broker. Consequences: Cannot publish message '{0}'", message.ToString());
                log.Error(error, ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                var error = String.Format("Unable to connect to RabbitMQ broker. Consequences: Cannot publish message '{0}'", message.ToString());
                log.Error(error, ex);
                return false;
            }
            //if (afterPublish != null) afterPublish(message);
            return true;
        }
    }
}
