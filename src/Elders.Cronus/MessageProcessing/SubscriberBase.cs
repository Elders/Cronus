﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.MessageProcessing
{
    public abstract class SubscriberBase : ISubscriber
    {
        protected readonly Type handlerType;
        protected readonly Workflow<HandleContext> handlerWorkflow;

        public SubscriberBase(Type handlerType, Workflow<HandleContext> handlerWorkflow)
        {
            this.handlerType = handlerType;
            this.handlerWorkflow = handlerWorkflow;

            bool hasDataContract = handlerType.HasAttribute<DataContractAttribute>();
            Id = hasDataContract ? handlerType.GetContractId() : handlerType.FullName;
        }

        public string Id { get; protected set; }

        public virtual void Process(CronusMessage message)
        {
            var context = new HandleContext(message, handlerType);
            handlerWorkflow.Run(context);
        }

        /// <summary>
        /// Gets all message types which this subscriber is interested in.
        /// </summary>
        /// <returns>Returns the message types.</returns>
        public abstract IEnumerable<Type> GetInvolvedMessageTypes();
    }
}
