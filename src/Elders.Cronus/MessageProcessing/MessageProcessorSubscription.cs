using System;

namespace Elders.Cronus.MessageProcessing
{
    public class MessageProcessorSubscription : IObserver<TransportMessage>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MessageProcessorSubscription));

        private IDisposable unsubscriber;
        private readonly Func<Type, object> handlerFactory;

        public MessageProcessorSubscription(Type messageType, Type messageHandlerType, Func<Type, object> handlerFactory)
        {
            MessageType = messageType;
            MessageHandlerType = messageHandlerType;
            this.handlerFactory = handlerFactory;
            Id = messageHandlerType.FullName;
        }
        public string Id { get; private set; }
        public Type MessageType { get; private set; }
        public Type MessageHandlerType { get; private set; }

        public virtual void OnNext(TransportMessage value)
        {
            dynamic handler = handlerFactory(MessageHandlerType);
            handler.Handle((dynamic)value.Payload);
        }

        public virtual void OnCompleted()
        {
            //Console.WriteLine("The Location Tracker has completed transmitting data to {0}.", this.Name);
            this.Unsubscribe();
        }

        public virtual void OnError(Exception ex)
        {

        }

        public virtual void Subscribe(MessageProcessor provider)
        {
            if (provider != null)
                unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            if (unsubscriber != null)
                unsubscriber.Dispose();
        }
    }
}