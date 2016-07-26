using Elders.Cronus.DomainModeling;
using Elders.Cronus.MessageProcessingMiddleware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Netflix
{
    public class HandleSubscriber<T, V> : ISubscriber
    {
        readonly MessageHandlerMiddleware handlerMiddleware;

        readonly Type handlerType;

        public HandleSubscriber(Type handlerType, ApplicationServiceMiddleware handlerMiddleware)
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

        public string Id { get; private set; }

        /// <summary>
        /// Gets the message types which the subscriber can process.
        /// </summary>
        public List<Type> MessageTypes { get; private set; }

        public void Process(CronusMessage message)
        {
            try
            {
                var context = new HandlerContext(message.Payload, handlerType);
                handlerMiddleware.Run(context);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private IEnumerable<Type> GetInvolvedMessageTypes(Type type)
        {
            var ieventHandler = typeof(V);
            var interfaces = type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler);
            foreach (var @interface in interfaces)
            {
                Type eventType = @interface.GetGenericArguments().FirstOrDefault();
                yield return eventType;
            }
        }
    }
}
