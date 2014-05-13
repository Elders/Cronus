using System.Collections.Generic;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus
{
    public class BatchConsumer<T> where T : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BatchConsumer<T>));

        private readonly IMessageProcessor<T> messageHandlers;

        public BatchConsumer(IMessageProcessor<T> messageHandlers)
        {
            this.messageHandlers = messageHandlers;
        }

        protected SafeBatchResult<T> BatchConsume(List<T> messages)
        {
            return messageHandlers.Handle(messages);
        }
    }
}