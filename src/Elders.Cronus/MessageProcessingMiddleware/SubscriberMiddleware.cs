using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Logging;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.MessageProcessingMiddleware
{
    public class SubscriberMiddleware : Middleware<Message>
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(SubscriberMiddleware));

        private readonly Type messageHandlerType;

        private IDisposable subscription;

        public Middleware<HandlerContext> MessageHandlerMiddleware { get; private set; }

        public SubscriberMiddleware(string name, Type messageType, Type handlerType, MessageHandlerMiddleware messageHandlerMiddleware)
        {
            MessageType = messageType;
            messageHandlerType = handlerType;
            Id = BuildId();
            Name = name;
            MessageHandlerMiddleware = messageHandlerMiddleware;
        }

        public string Id { get; private set; }

        /// <summary>
        /// Gets the type of the message subscribing to.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public Type MessageType { get; private set; }

        public string Name { get; set; }

        protected override void Run(Execution<Message> middlewareControl)
        {
            var message = middlewareControl.Context;
            MessageHandlerMiddleware.Run(new HandlerContext(message.Payload, messageHandlerType));
            log.Info(() => message.Payload.ToString());
            log.Debug(() => "HANDLE => " + messageHandlerType.Name + "( " + BuildDebugLog(message) + " )");
        }

        string BuildDebugLog(Message message)
        {
            IMessage realMessage = message.Payload as IMessage;
            if (ReferenceEquals(null, realMessage))
                return realMessage + $" |=> {Id}";

            return realMessage.ToString($"{message.Payload.ToString()} |=> {Id}");
        }

        public void OnCompleted()
        {
            this.Unsubscribe();
        }

        public void Subscribe(MessageSubscriptionsMiddleware provider)
        {
            if (provider != null)
            {
                subscription = provider.Subscribe(this);
                log.Debug(() => $"Subscriber '{Name}' with id {Id} and handler {messageHandlerType} subscribed for message {MessageType} in {provider}");
            }
        }

        public void Unsubscribe()
        {
            if (subscription != null)
            {
                subscription.Dispose();
            }
        }

        protected string BuildId()
        {
            return messageHandlerType.FullName;
        }
    }


    public class HandlerContext
    {
        public HandlerContext(object message, Type handlerType)
        {
            Message = message;
            HandlerType = handlerType;
        }

        public object Message { get; private set; }

        public Type HandlerType { get; private set; }
    }
}
