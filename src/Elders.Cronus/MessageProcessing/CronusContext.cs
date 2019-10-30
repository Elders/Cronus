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
                if (string.IsNullOrEmpty(tenant)) throw new ArgumentException($"Unknown tenant. CronusContext is not properly built. Please call `Initialize(...)` and make sure that you properly configured `cronus_tenants`. More info at https://github.com/Elders/Cronus/blob/master/doc/Configuration.md");
                return tenant;
            }
            set
            {
                tenant = value;
            }
        }

        public bool IsNotInitialized => string.IsNullOrEmpty(tenant) || ServiceProvider is null;

        public bool IsInitialized => IsNotInitialized == false;

        public IServiceProvider ServiceProvider { get; set; }
    }
}
