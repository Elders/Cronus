using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Logging;

namespace Elders.Cronus
{
    public abstract class Publisher<TMessage> : IPublisher<TMessage> where TMessage : IMessage
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(Publisher<TMessage>));

        protected abstract bool PublishInternal(TMessage message, Dictionary<string, string> messageHeaders);

        public bool Publish(TMessage message, Dictionary<string, string> messageHeaders)
        {
            try
            {
                PublishInternal(message, messageHeaders);
                log.Info(() => "PUBLISH => " + message);
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorException(ex.Message, ex);
                return false;
            }
        }
    }
}
