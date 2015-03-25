using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventStore;

namespace Elders.Cronus.MessageProcessing
{
    public abstract class MessageProcessorSubscription : IObserver<Message>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MessageProcessorSubscription));

        private IDisposable unsubscriber;

        public MessageProcessorSubscription(Type messageType, Type handlerType)
        {
            MessageType = messageType;
            HandlerType = handlerType;
            Id = BuildId();
        }

        public string Id { get; private set; }

        /// <summary>
        /// Gets the type of the message subscribing to.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public Type MessageType { get; private set; }

        public Type HandlerType { get; private set; }

        public void OnNext(Message value)
        {
            InternalOnNext(value);
            log.Info("HANDLE => " + HandlerType.Name + "( " + value.Payload + " )");
        }

        public void OnCompleted()
        {
            this.Unsubscribe();
            InternalOnCompleted();
        }

        public void OnError(Exception ex)
        {
            InternalOnError(ex);
        }

        public void Subscribe(MessageProcessor provider)
        {
            if (provider != null)
            {
                unsubscriber = provider.Subscribe(this);
                InternalOnSubscribe(provider);
            }
        }

        public void Unsubscribe()
        {
            if (unsubscriber != null)
            {
                unsubscriber.Dispose();
                InternalOnUnsubscribe();
            }
        }

        protected virtual string BuildId()
        {
            return HandlerType.FullName;
        }

        protected abstract void InternalOnNext(Message value);
        protected virtual void InternalOnCompleted() { }
        protected virtual void InternalOnError(Exception ex) { }
        protected virtual void InternalOnSubscribe(MessageProcessor provider) { }
        protected virtual void InternalOnUnsubscribe() { }
    }

    public class ApplicationServiceSubscription : MessageProcessorSubscription
    {
        private readonly IHandlerFactory handlerFactory;

        private readonly IAggregateRepository aggregateRepository;

        public ApplicationServiceSubscription(Type messageType, IHandlerFactory factory, IAggregateRepository aggregateRepository, IPublisher<IEvent> eventPublisher)
            : base(messageType, factory.MessageHandlerType)
        {
            handlerFactory = factory;
            this.aggregateRepository = new RepositoryProxy(aggregateRepository, eventPublisher);
        }

        protected override void InternalOnNext(Message value)
        {
            dynamic handler = handlerFactory
                .Create()
                .AssignPropertySafely<IAggregateRootApplicationService>(x => x.Repository = aggregateRepository);
            handler.Handle((dynamic)value.Payload);
        }

        class RepositoryProxy : IAggregateRepository
        {
            private readonly IAggregateRepository aggregateRepository;
            private readonly IPublisher<IEvent> eventPublisher;

            public RepositoryProxy(IAggregateRepository repository, IPublisher<IEvent> eventPublisher)
            {
                this.aggregateRepository = repository;
                this.eventPublisher = eventPublisher;
            }

            public AR Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
            {
                return aggregateRepository.Load<AR>(id);
            }

            public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
            {
                aggregateRepository.Save<AR>(aggregateRoot);

                var events = aggregateRoot.UncommittedEvents.ToList();
                for (int i = 0; i < events.Count; i++)
                {
                    eventPublisher.Publish(events[i], BuildHeaders(aggregateRoot, i));
                }
            }

            private Dictionary<string, string> BuildHeaders(IAggregateRoot aggregateRoot, int eventPosition)
            {
                Dictionary<string, string> messageHeaders = new Dictionary<string, string>();

                messageHeaders.Add("ar_id", aggregateRoot.State.Id.ToString());
                messageHeaders.Add("ar_revision", aggregateRoot.Revision.ToString());
                messageHeaders.Add("publish_timestamp", DateTime.UtcNow.ToString());
                messageHeaders.Add("event_position", eventPosition.ToString());

                return messageHeaders;
            }
        }
    }

    public class ProjectionSubscription : MessageProcessorSubscription
    {
        private readonly IHandlerFactory handlerFactory;

        public ProjectionSubscription(Type messageType, IHandlerFactory factory)
            : base(messageType, factory.MessageHandlerType)
        {
            handlerFactory = factory;
        }

        protected override void InternalOnNext(Message value)
        {
            dynamic handler = handlerFactory.Create();
            handler.Handle((dynamic)value.Payload);
        }
    }

    public class PortSubscription : MessageProcessorSubscription
    {
        private readonly IHandlerFactory handlerFactory;
        private readonly IPublisher<ICommand> commandPublisher;

        public PortSubscription(Type messageType, IHandlerFactory factory, IPublisher<ICommand> commandPublisher)
            : base(messageType, factory.MessageHandlerType)
        {
            handlerFactory = factory;
            this.commandPublisher = commandPublisher;
        }

        protected override void InternalOnNext(Message value)
        {
            dynamic handler = handlerFactory
                .Create()
                .AssignPropertySafely<IPort>(x => x.CommandPublisher = commandPublisher); ;
            handler.Handle((dynamic)value.Payload);
        }
    }
}
