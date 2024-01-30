using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.MessageProcessing
{
    public abstract class SubscriberBase : ISubscriber
    {
        protected readonly Type handlerType;
        protected readonly Workflow<HandleContext> handlerWorkflow;
        private readonly ILogger logger;

        public SubscriberBase(Type handlerType, Workflow<HandleContext> handlerWorkflow, ILogger logger)
        {
            this.handlerType = handlerType;
            this.handlerWorkflow = handlerWorkflow;
            this.logger = logger;
            bool hasDataContract = handlerType.HasAttribute<DataContractAttribute>();
            Id = hasDataContract ? handlerType.GetContractId() : handlerType.FullName;
        }

        public string Id { get; protected set; }

        public Task ProcessAsync(CronusMessage message)
        {
            try
            {
                var context = new HandleContext(message, handlerType);
                return handlerWorkflow.RunAsync(context);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Subscriber crashed!");

                return Task.FromException(ex);
            }
        }

        /// <summary>
        /// Gets all message types which this subscriber is interested in.
        /// </summary>
        /// <returns>Returns the message types.</returns>
        public abstract IEnumerable<Type> GetInvolvedMessageTypes();
    }
}
