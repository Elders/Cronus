using System;
using System.Collections.Generic;
using System.IO;
using NMSD.Cronus.Multithreading.Work;
using NMSD.Cronus.UnitOfWork;
using RabbitMQ.Client.Exceptions;

namespace NMSD.Cronus.Messaging
{
    public abstract class Publisher<TMessage> : IPublisher<TMessage>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Publisher<TMessage>));

        protected abstract bool PublishInternal(TMessage message);

        public bool Publish(TMessage message)
        {
            //if (beforePublish != null) beforePublish(message);
            try
            {
                PublishInternal(message);
                log.Info("PUBLISH => " + message.ToString());
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

        class InMemoryConsumer : BaseInMemoryConsumer<TMessage, THandler>
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
                    bool handled = false;
                    IUnitOfWorkPerBatch unitOfWork = consumer.UnitOfWorkFactory.NewBatch();
                    if (unitOfWork != null)
                    {
                        unitOfWork.Begin();
                    }
                    consumer.Handle(message, unitOfWork);
                    if (unitOfWork != null)
                    {
                        unitOfWork.Commit();
                        unitOfWork.Dispose();
                        unitOfWork = null;
                    }
                    return handled;
                }
            }
        }
    }
}