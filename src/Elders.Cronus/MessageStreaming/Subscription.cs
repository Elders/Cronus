using System;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus
{
    public class Subscription : IObserver<TransportMessage>
    {
        private IDisposable unsubscriber;
        private readonly Func<Type, object> handlerFactory;

        public Subscription(Type messageType, Type messageHandlerType, Func<Type, object> handlerFactory)
        {
            MessageType = messageType;
            MessageHandlerType = messageHandlerType;
            this.handlerFactory = handlerFactory;
        }

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

        public virtual void OnError(Exception e)
        {
            //Console.WriteLine("{0}: The location cannot be determined.", this.Name);
        }

        public virtual void Subscribe(MessageStream provider)
        {
            if (provider != null)
                unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }
    }
}