using Elders.Cronus.DomainModeling;
using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessingMiddleware;
using Elders.Cronus.Middleware;
using Elders.Cronus.Pipeline.CircuitBreaker;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.Netflix
{
    public class SubscriptionMiddleware
    {
        private ConcurrentBag<ISubscriber> subscribers;

        public SubscriptionMiddleware()
        {
            subscribers = new ConcurrentBag<ISubscriber>();
        }

        public void Subscribe(ISubscriber subscriber)
        {
            if (ReferenceEquals(null, subscriber)) throw new ArgumentNullException(nameof(subscriber));
            if (ReferenceEquals(null, subscriber.MessageTypes)) throw new ArgumentNullException(nameof(subscriber.MessageTypes));
            if (subscriber.MessageTypes.Any() == false) throw new ArgumentException($"Subscirber {subscriber.Id} does not care about any message types. Any reason?");
            if (subscribers.Any(x => x.Id == subscriber.Id)) throw new ArgumentException($"There is already subscriber with id {subscriber.Id}");

            subscribers.Add(subscriber);
        }

        public IEnumerable<ISubscriber> GetInterestedSubscribers(CronusMessage message)
        {
            return Subscribers.Where(subscriber => subscriber.MessageTypes.Contains(message.Type));
        }

        public IEnumerable<ISubscriber> Subscribers { get { return subscribers.ToList(); } }

        public void OnSubscribe(Middleware<ISubscriber> onSubscribe)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeAll()
        {
            subscribers = new ConcurrentBag<ISubscriber>();
        }
    }

    public class ProjectionSubscriber : ISubscriber
    {
        static ILog log = LogProvider.GetLogger(typeof(ProjectionSubscriber));

        readonly IEndpointCircuitBreaker legacyCircuitBreaker;

        readonly MessageHandlerMiddleware handlerMiddleware;

        readonly Type handlerType;

        public ProjectionSubscriber(IEndpointCircuitBreaker legacyCircuitBreaker, Type handlerType, ProjectionsMiddleware handlerMiddleware)
        {
            if (handlerMiddleware == null) throw new ArgumentNullException(nameof(handlerMiddleware));
            if (ReferenceEquals(null, legacyCircuitBreaker)) throw new ArgumentNullException(nameof(legacyCircuitBreaker));

            this.handlerType = handlerType;
            this.legacyCircuitBreaker = legacyCircuitBreaker;
            this.handlerMiddleware = handlerMiddleware;
            if ((handlerType is IProjection) == false)
                throw new ArgumentException($"'{handlerType.FullName}' does not implement IProjection");
            Id = handlerType.FullName;
            MessageTypes = GetInvolvedMessageTypes(handlerType).ToList();

        }

        public string Id { get; private set; }

        /// <summary>
        /// Gets the message types which the subscriber can process.
        /// </summary>
        public List<string> MessageTypes { get; private set; }

        public void Process(CronusMessage message)
        {
            if (legacyCircuitBreaker.RetryStrategy.AllowsProcessing(Id, message))
            {
                try
                {
                    var context = new HandlerContext(message.Payload, handlerType);
                    handlerMiddleware.Run(context);
                }
                catch (Exception ex)
                {
                    message.Errors.Add(new FeedError()
                    {
                        Origin = new ErrorOrigin(Id, ErrorOriginType.MessageHandler),
                        Error = new SerializableException(ex)
                    });
                }
                finally
                {
                    legacyCircuitBreaker.PostConsume(message);
                }
            }
            else
            {
                log.Warn(() => "The retry strategy does not allow Cronus message to be handled more than 5 times or something else.");
            }
        }

        private IEnumerable<string> GetInvolvedMessageTypes(Type handlerType)
        {
            var ieventHandler = typeof(IEventHandler<>);
            var interfaces = handlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler);
            foreach (var @interface in interfaces)
            {
                Type eventType = @interface.GetGenericArguments().FirstOrDefault();
                var contractName = eventType.GetAttrubuteValue<DataContractAttribute, string>(x => x.Name);
                yield return contractName;
            }
        }
    }

    public class PortSubscriber : ISubscriber
    {
        readonly IEndpointCircuitBreaker legacyCircuitBreaker;

        readonly MessageHandlerMiddleware handlerMiddleware;

        readonly Type handlerType;

        public PortSubscriber(IEndpointCircuitBreaker legacyCircuitBreaker, Type handlerType, ProjectionsMiddleware handlerMiddleware)
        {
            if (handlerMiddleware == null) throw new ArgumentNullException(nameof(handlerMiddleware));
            if (ReferenceEquals(null, legacyCircuitBreaker)) throw new ArgumentNullException(nameof(legacyCircuitBreaker));

            this.handlerType = handlerType;
            this.legacyCircuitBreaker = legacyCircuitBreaker;
            this.handlerMiddleware = handlerMiddleware;
            if ((handlerType is IPort) == false)
                throw new ArgumentException($"'{handlerType.FullName}' does not implement IProjection");
            Id = handlerType.FullName;
            MessageTypes = GetInvolvedMessageTypes(handlerType).ToList();

        }

        public string Id { get; private set; }

        /// <summary>
        /// Gets the message types which the subscriber can process.
        /// </summary>
        public List<string> MessageTypes { get; private set; }

        public void Process(CronusMessage message)
        {
            try
            {
                var context = new HandlerContext(message.Payload, handlerType);
                handlerMiddleware.Run(context);
            }
            catch (Exception ex)
            {
                message.Errors.Add(new FeedError()
                {
                    Origin = new ErrorOrigin(Id, ErrorOriginType.MessageHandler),
                    Error = new SerializableException(ex)
                });
                legacyCircuitBreaker.ErrorStrategy.Handle(message);
            }
        }

        private IEnumerable<string> GetInvolvedMessageTypes(Type type)
        {
            var ieventHandler = typeof(IEventHandler<>);
            var interfaces = type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler);
            foreach (var @interface in interfaces)
            {
                Type eventType = @interface.GetGenericArguments().FirstOrDefault();
                var contractName = eventType.GetAttrubuteValue<DataContractAttribute, string>(x => x.Name);
                yield return contractName;
            }
        }
    }
}
