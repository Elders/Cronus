using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using System.Linq;

namespace Elders.Cronus
{
    public class BatchConsumer<TContract> where TContract : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BatchConsumer<TContract>));

        private readonly IMessageProcessor<TContract> messageHandlers;

        public BatchConsumer(IMessageProcessor<TContract> messageHandlers)
        {
            this.messageHandlers = messageHandlers;
        }

        protected ISafeBatchResult<TransportMessage> BatchConsume(List<TransportMessage> messages)
        {
            return messageHandlers.Handle(messages);
        }
    }
}