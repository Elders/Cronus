using System;

namespace Elders.Cronus.MessageProcessing
{
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
