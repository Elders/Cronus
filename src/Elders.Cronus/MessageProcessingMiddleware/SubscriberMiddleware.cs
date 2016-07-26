using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Logging;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.MessageProcessingMiddleware
{
    public class SubscriberMiddleware : Middleware<IMessage>
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(SubscriberMiddleware));

        readonly Type messageHandlerType;

        IDisposable subscription;

        public Middleware<HandlerContext> MessageHandlerMiddleware { get; private set; }

        public SubscriberMiddleware(string name, Type messageType, Type handlerType, MessageHandlerMiddleware messageHandlerMiddleware)
        {
            Name = name;
            MessageType = messageType;
            messageHandlerType = handlerType;
            Id = BuildId();
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

        protected override void Run(Execution<IMessage> execution)
        {
            var message = execution.Context;
            MessageHandlerMiddleware.Run(new HandlerContext(message, messageHandlerType));
            log.Info(() => message.ToString());
            log.Debug(() => "HANDLE => " + messageHandlerType.Name + "( " + BuildDebugLog(message) + " )");
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
                log.Debug(() => $"Subscriber '{Name}' with id '{Id}' and handler '{messageHandlerType}' subscribed for message '{MessageType}' in '{provider}'");
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

        private string BuildDebugLog(IMessage message)
        {
            if (ReferenceEquals(null, message))
                return message + $" |=> {Id}";

            return message.ToString($"{message.ToString()} |=> {Id}");
        }
    }


    public class HandlerContext
    {
        public HandlerContext(IMessage message, Type handlerType)
        {
            Message = message;
            HandlerType = handlerType;
        }

        public IMessage Message { get; private set; }

        public Type HandlerType { get; private set; }
    }
}
