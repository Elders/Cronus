using System;

namespace NMSD.Cronus.Messaging
{
    public abstract class Publisher<TMessage> : IPublisher<TMessage>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Publisher<TMessage>));

        protected abstract bool PublishInternal(TMessage message);

        public bool Publish(TMessage message)
        {
            try
            {
                PublishInternal(message);
                log.Info("PUBLISH => " + message.ToString());
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