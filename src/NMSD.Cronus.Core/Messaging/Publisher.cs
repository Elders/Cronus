using System;
using System.IO;
using RabbitMQ.Client.Exceptions;

namespace NMSD.Cronus.Core.Messaging
{
    public abstract class Publisher<TMessage> : IPublisher<TMessage>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Publisher<TMessage>));

        protected Action<TMessage> beforePublish;

        protected Action<TMessage> afterPublish;

        public void SetBeforePublishAction(Action<TMessage> action)
        {
            beforePublish = action;
        }

        public void SetAfterPublishAction(Action<TMessage> action)
        {
            afterPublish = action;
        }

        protected abstract bool PublishInternal(TMessage message);

        public bool Publish(TMessage message)
        {
            //if (beforePublish != null) beforePublish(message);
            try
            {
                PublishInternal(message);
                log.Info("PUBLISH => " + message.ToString());
            }
            catch (AlreadyClosedException ex)
            {
                var error = String.Format("Unable to connect to RabbitMQ broker. Consequences: Cannot publish message '{0}'", message.ToString());
                log.Error(error, ex);
                return false;
            }
            catch (IOException ex)
            {
                var error = String.Format("Unable to connect to RabbitMQ broker. Consequences: Cannot publish message '{0}'", message.ToString());
                log.Error(error, ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                var error = String.Format("Unable to connect to RabbitMQ broker. Consequences: Cannot publish message '{0}'", message.ToString());
                log.Error(error, ex);
                return false;
            }
            //if (afterPublish != null) afterPublish(message);
            return true;
        }
    }
}
