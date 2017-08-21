using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Logging;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.MessageProcessing
{
    public class HandleSubscriber<T, V> : ISubscriber
    {
        static ILog log = LogProvider.GetLogger(typeof(HandleSubscriber<,>));

        readonly Middleware<HandleContext> handlerMiddleware;

        readonly Type handlerType;

        public HandleSubscriber(Type handlerType, Middleware<HandleContext> handlerMiddleware)
        {
            if (handlerMiddleware == null) throw new ArgumentNullException(nameof(handlerMiddleware));

            this.handlerType = handlerType;
            this.handlerMiddleware = handlerMiddleware;
            var expectedHandlerType = typeof(T);
            if (expectedHandlerType.IsAssignableFrom(handlerType) == false)
                throw new ArgumentException($"'{handlerType.FullName}' does not implement {expectedHandlerType.FullName}");
            Id = handlerType.FullName;
            MessageTypes = GetInvolvedMessageTypes(handlerType).ToList();
        }

        private string BuildDebugLog(CronusMessage message)
        {
            return message.Payload.ToString($"{message.Payload.ToString()} |=> @subscriber '{Id}'");
        }

        public string Id { get; private set; }

        /// <summary>
        /// Gets the message types which the subscriber can process.
        /// </summary>
        public List<Type> MessageTypes { get; private set; }

        public void Process(CronusMessage message)
        {
            var context = new HandleContext(message, handlerType);
            handlerMiddleware.Run(context);
            log.Info(() => message.Payload.ToString());
            log.Debug(() => "HANDLE => " + handlerType.Name + "( " + BuildDebugLog(message) + " )");
        }

        IEnumerable<Type> GetInvolvedMessageTypes(Type type)
        {
            var ieventHandler = typeof(V).GetGenericTypeDefinition();
            var interfaces = type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler);
            foreach (var @interface in interfaces)
            {
                Type eventType = @interface.GetGenericArguments().FirstOrDefault();
                yield return eventType;
            }
        }
    }

    public class SagaSubscriber : ISubscriber
    {
        static ILog log = LogProvider.GetLogger(typeof(SagaSubscriber));

        readonly Middleware<HandleContext> handlerMiddleware;

        readonly Type handlerType;

        public SagaSubscriber(Type handlerType, Middleware<HandleContext> handlerMiddleware)
        {
            if (handlerMiddleware == null) throw new ArgumentNullException(nameof(handlerMiddleware));

            this.handlerType = handlerType;
            this.handlerMiddleware = handlerMiddleware;
            var expectedHandlerType = typeof(ISaga);
            if (expectedHandlerType.IsAssignableFrom(handlerType) == false)
                throw new ArgumentException($"'{handlerType.FullName}' does not implement {expectedHandlerType.FullName}");
            Id = handlerType.FullName;
            MessageTypes = GetInvolvedMessageTypes(handlerType).ToList();
        }

        private string BuildDebugLog(CronusMessage message)
        {
            return message.Payload.ToString($"{message.Payload.ToString()} |=> @subscriber '{Id}'");
        }

        public string Id { get; private set; }

        /// <summary>
        /// Gets the message types which the subscriber can process.
        /// </summary>
        public List<Type> MessageTypes { get; private set; }

        public void Process(CronusMessage message)
        {
            var context = new HandleContext(message, handlerType);
            handlerMiddleware.Run(context);
            log.Info(() => message.Payload.ToString());
            log.Debug(() => "HANDLE => " + handlerType.Name + "( " + BuildDebugLog(message) + " )");
        }

        IEnumerable<Type> GetInvolvedMessageTypes(Type type)
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
}
