using System;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus
{
    public abstract class Publisher<TMessage> : IPublisher<TMessage>
        where TMessage : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Publisher<TMessage>));

        protected abstract bool PublishInternal(TMessage message);

        public bool Publish(TMessage message)
        {
            try
            {
                PublishInternal(message);
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

        public bool Publish(IMessage message)
        {
            return Publish((TMessage)message);
        }
    }

    public interface IConsumer<T> where T : IMessage
    {
        bool Consume(T message);
    }
}