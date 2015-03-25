using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus
{
    public abstract class Publisher<TMessage> : IPublisher<TMessage> where TMessage : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Publisher<TMessage>));

        protected abstract bool PublishInternal(TMessage message, Dictionary<string, string> messageHeaders);

        public bool Publish(TMessage message, Dictionary<string, string> messageHeaders)
        {
            try
            {
                PublishInternal(message, messageHeaders);
                if (log.IsInfoEnabled)
                    log.Info("PUBLISH => " + message);
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return false;
            }
        }
    }
}
