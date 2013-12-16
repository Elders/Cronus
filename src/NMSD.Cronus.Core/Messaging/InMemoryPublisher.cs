using System;
using System.Collections.Generic;
using NMSD.Cronus.Core.Multithreading.Work;

namespace NMSD.Cronus.Core.Messaging
{
    public abstract class InMemoryBus<TMessage, THandler> : IPublisher<TMessage>, IConsumer<THandler>
        where TMessage : IMessage
        where THandler : IMessageHandler
    {
        InMemoryConsumer consumer;

        IPublisher<TMessage> publisher;

        public InMemoryBus()
        {
            consumer = new InMemoryConsumer();
            publisher = new InMemoryConsumer.InMemoryPublisher(consumer);
        }

        public virtual bool Publish(TMessage message)
        {
            return publisher.Publish(message);
        }

        public void RegisterHandler(Type messageType, Type handlerType, Func<Type, THandler> handlerFactory)
        {
            consumer.RegisterHandler(messageType, handlerType, handlerFactory);
        }

        public void Start(int numberOfWorkers) { }

        public void Stop() { }

        class InMemoryConsumer : Consumer<TMessage, THandler>
        {
            public override void Start(int numberOfWorkers) { }

            public override void Stop() { }

            internal class InMemoryPublisher : Publisher<TMessage>
            {
                private readonly InMemoryConsumer consumer;

                public InMemoryPublisher(InMemoryConsumer consumer)
                {
                    this.consumer = consumer;
                }

                protected override bool PublishInternal(TMessage message)
                {
                    return consumer.Handle(message);
                }
            }
        }
    }
}