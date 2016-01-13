﻿using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Logging;

namespace Elders.Cronus.InMemory
{
    public class InMemoryPublisher<TContract> : Publisher<TContract> where TContract : IMessage
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(InMemoryPublisher<TContract>));

        IMessageProcessor messageProcessor;

        public InMemoryPublisher(IMessageProcessor messageProcessor)
        {
            this.messageProcessor = messageProcessor;
        }

        protected override bool PublishInternal(TContract message, Dictionary<string, string> messageHeaders)
        {
            var result = messageProcessor.Feed(new List<TransportMessage>() { new TransportMessage(new Message(message)) });
            if (result.FailedMessages != null && result.FailedMessages.Count() > 0)
            {
                foreach (var msg in result.FailedMessages)
                {
                    log.ErrorException(msg.Payload.ToString(), msg.Errors.First().Error);
                }
                return false;
            }
            return true;
        }
    }
}
