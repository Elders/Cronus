using System;

namespace Elders.Cronus.MessageProcessing
{
    public class HandlerContext
    {
        public HandlerContext(IMessage message, object handlerInstance, CronusMessage cronusMessage)
        {
            Message = message;
            HandlerInstance = handlerInstance;
            CronusMessage = cronusMessage;
        }
        public IMessage Message { get; private set; }

        public object HandlerInstance { get; private set; }

        public CronusMessage CronusMessage { get; private set; }
    }

    public class CronusContext
    {
        private string tenant;

        public string Tenant
        {
            get
            {
                if (string.IsNullOrEmpty(tenant)) throw new ArgumentException("Unknown tenant. CronusContext is not properly built. Make sure that someone properly resolves the current tenant and sets it to this instance.");
                return tenant;
            }
            set => tenant = value;
        }
    }
}
