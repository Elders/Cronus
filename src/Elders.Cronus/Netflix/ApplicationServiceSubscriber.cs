using Elders.Cronus.DomainModeling;
using Elders.Cronus.MessageProcessingMiddleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Elders.Cronus.Netflix
{
    public class ApplicationServiceSubscriber : ISubscriber
    {
        readonly MessageHandlerMiddleware handlerMiddleware;

        readonly Type handlerType;

        public ApplicationServiceSubscriber(Type handlerType, ApplicationServiceMiddleware handlerMiddleware)
        {
            if (handlerMiddleware == null) throw new ArgumentNullException(nameof(handlerMiddleware));

            this.handlerType = handlerType;
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
            }
        }

        private IEnumerable<string> GetInvolvedMessageTypes(Type type)
        {
            var ieventHandler = typeof(ICommandHandler<>);
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