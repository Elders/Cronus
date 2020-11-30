using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.MessageProcessing
{
    public class HandlerSubscriber : ISubscriber
    {
        protected readonly Workflow<HandleContext> handlerWorkflow;

        protected readonly Type handlerType;

        public HandlerSubscriber(Type handlerType, Workflow<HandleContext> handlerWorkflow, string subscriberId = null)
        {
            if (handlerType is null) throw new ArgumentNullException(nameof(handlerType));
            if (handlerWorkflow is null) throw new ArgumentNullException(nameof(handlerWorkflow));

            this.handlerType = handlerType;
            this.handlerWorkflow = handlerWorkflow;
            bool hasDataContract = handlerType.HasAttribute<DataContractAttribute>();
            Id = subscriberId ?? (hasDataContract ? handlerType.GetContractId() : handlerType.FullName);
        }

        public string Id { get; private set; }

        public virtual void Process(CronusMessage message)
        {
            var context = new HandleContext(message, handlerType);
            handlerWorkflow.Run(context);
        }

        public virtual IEnumerable<Type> GetInvolvedMessageTypes()
        {
            Type baseMessageType = typeof(IMessage);

            return handlerType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericArguments().Length == 1 && (baseMessageType.IsAssignableFrom(x.GetGenericArguments()[0])))
                .Select(@interface => @interface.GetGenericArguments()[0])
                .Distinct();
        }
    }
}
