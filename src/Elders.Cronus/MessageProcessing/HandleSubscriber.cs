using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Logging;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.MessageProcessing
{
    public class HandlerSubscriber : ISubscriber
    {
        static ILog log = LogProvider.GetLogger(typeof(HandlerSubscriber));

        protected readonly Middleware<HandleContext> handlerMiddleware;

        protected readonly Type handlerType;

        public HandlerSubscriber(Type handlerType, Middleware<HandleContext> handlerMiddleware, string subscriberId = null)
        {
            if (ReferenceEquals(null, handlerType)) throw new ArgumentNullException(nameof(handlerType));
            if (ReferenceEquals(null, handlerMiddleware)) throw new ArgumentNullException(nameof(handlerMiddleware));

            this.handlerType = handlerType;
            this.handlerMiddleware = handlerMiddleware;
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

        public virtual IEnumerable<Type> GetInvolvedMessageTypes()
        {
            return handlerType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericArguments().Length == 1 && (typeof(IMessage).IsAssignableFrom(x.GetGenericArguments().Single())))
                .Select(@interface => @interface.GetGenericArguments().Single())
                .Distinct();
        }
    }
}
