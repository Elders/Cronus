using System;
using Microsoft.Extensions.DependencyInjection;

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

    public sealed class CronusContext
    {
        private string tenant;

        public string Tenant
        {
            get
            {
                if (string.IsNullOrEmpty(tenant)) throw new ArgumentException("Unknown tenant. CronusContext is not properly built. Make sure that someone properly resolves the current tenant and sets it to this instance.");
                return tenant;
            }
            private set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Tenant));
                tenant = value;
            }
        }

        public bool IsNotInitialized => string.IsNullOrEmpty(tenant) || ServiceProvider is null;

        public IServiceProvider ServiceProvider { get; private set; }

        public void Initialize(string tenant, IServiceProvider serviceProvider)
        {
            Tenant = tenant;
            ServiceProvider = serviceProvider;
        }
    }
}
