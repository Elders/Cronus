using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline;

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

   

    public static class ObjectExtensions
    {
        public static object AssignPropertySafely<TContract>(this object self, Action<TContract> assignProperty)
        {
            var canProceed = typeof(TContract).IsAssignableFrom(self.GetType());
            if (canProceed)
            {
                var contract = (TContract)self;
                assignProperty(contract);
                self = contract;
            }
            return self;
        }
    }
}