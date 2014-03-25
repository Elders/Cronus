using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus
{
    public class BatchConsumer<T>
            where T : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BatchConsumer<T>));

        private readonly MessageHandlerCollection<T> messageHandlers;

        public BatchConsumer(MessageHandlerCollection<T> messageHandlers)
        {
            this.messageHandlers = messageHandlers;
        }

        public SafeBatchResult<T> Consume(List<T> messages)
        {
            try
            {
                return messageHandlers.Handle(messages);
            }
            catch (Exception ex)
            {
                //string error = String.Format("Error while handling message '{0}'", message.ToString());
                log.Error(ex);
                throw;
            }
        }
    }
}