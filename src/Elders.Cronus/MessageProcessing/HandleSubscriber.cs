using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Elders.Cronus.Cluster;
using Elders.Cronus.Logging;
using Elders.Cronus.Middleware;
using Elders.Cronus.Projections;

namespace Elders.Cronus.MessageProcessing
{
    public abstract class BaseHandlerSubscriber<T> : ISubscriber
    {
        static ILog log = LogProvider.GetLogger(typeof(BaseHandlerSubscriber<>));

        protected readonly Middleware<HandleContext> handlerMiddleware;

        protected readonly Type handlerType;

        public BaseHandlerSubscriber(Type handlerType, Middleware<HandleContext> handlerMiddleware, string subscriberId = null)
        {
            if (ReferenceEquals(null, handlerType)) throw new ArgumentNullException(nameof(handlerType));
            if (ReferenceEquals(null, handlerMiddleware)) throw new ArgumentNullException(nameof(handlerMiddleware));

            this.handlerType = handlerType;
            this.handlerMiddleware = handlerMiddleware;
            var expectedHandlerType = typeof(T);
            if (expectedHandlerType.IsAssignableFrom(handlerType) == false)
                throw new ArgumentException($"'{handlerType.FullName}' does not implement {expectedHandlerType.FullName}");
            Id = subscriberId ?? handlerType.FullName;
        }

        protected string BuildDebugLog(CronusMessage message)
        {
            return message.Payload.ToString($"{message.Payload.ToString()} |=> @subscriber '{Id}'");
        }

        public string Id { get; private set; }

        public virtual void Process(CronusMessage message)
        {
            var context = new HandleContext(message, handlerType);
            handlerMiddleware.Run(context);
            log.Info(() => message.Payload.ToString());
            log.Debug(() => "HANDLE => " + handlerType.Name + "( " + BuildDebugLog(message) + " )");
        }

        protected abstract IEnumerable<Type> GetInvolvedMessageTypes(Type type);

        public IEnumerable<Type> GetInvolvedMessageTypes()
        {
            return GetInvolvedMessageTypes(handlerType);
        }
    }

    public class HandlerSubscriber<THandler, V> : BaseHandlerSubscriber<THandler>
    {
        static ILog log = LogProvider.GetLogger(typeof(HandlerSubscriber<,>));

        public HandlerSubscriber(Type handlerType, Middleware<HandleContext> handlerMiddleware, string subscriberId = null)
            : base(handlerType, handlerMiddleware, subscriberId)
        {
        }

        protected override IEnumerable<Type> GetInvolvedMessageTypes(Type type)
        {
            var ieventHandler = typeof(V).GetGenericTypeDefinition();
            var interfaces = handlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler);
            foreach (var @interface in interfaces)
            {
                Type eventType = @interface.GetGenericArguments().FirstOrDefault();
                yield return eventType;
            }
        }
    }

    public class SagaSubscriber : BaseHandlerSubscriber<ISaga>
    {
        public SagaSubscriber(Type handlerType, Middleware<HandleContext> handlerMiddleware, string subscriberId = null)
            : base(handlerType, handlerMiddleware, subscriberId)
        {
        }

        protected override IEnumerable<Type> GetInvolvedMessageTypes(Type type)
        {
            var ieventHandler = typeof(IEventHandler<>);
            var iSagaHandler = typeof(ISagaTimeoutHandler<>);
            var interfaces = type.GetInterfaces().Where(x => x.IsGenericType && (x.GetGenericTypeDefinition() == ieventHandler || x.GetGenericTypeDefinition() == iSagaHandler));
            foreach (var @interface in interfaces)
            {
                Type eventType = @interface.GetGenericArguments().FirstOrDefault();
                yield return eventType;
            }
        }
    }

    public class SystemServiceSubscriber : HandlerSubscriber<ISystemService, ICommandHandler<ICommand>>
    {
        public SystemServiceSubscriber(Type handlerType, Middleware<HandleContext> handlerMiddleware)
            : base(handlerType, handlerMiddleware)
        {
        }
    }

    public class SystemProjectionSubscriber : HandlerSubscriber<ISystemProjection, IEventHandler<IEvent>>
    {
        public SystemProjectionSubscriber(Type handlerType, Middleware<HandleContext> handlerMiddleware, string nodeName)
            : base(handlerType, handlerMiddleware, nodeName)
        {
        }
    }
}
