using System;
using System.Collections.Generic;

namespace Elders.Cronus.MessageProcessing
{
    public sealed class CronusContext
    {
        private string tenant;

        public CronusContext()
        {
            Trace = new Dictionary<string, object>();
        }

        public string Tenant
        {
            get
            {
                if (string.IsNullOrEmpty(tenant)) throw new ArgumentException($"Unknown tenant. CronusContext is not properly built. Please call `Initialize(...)` and make sure that you have properly configured `cronus:tenants`. More info at https://github.com/Elders/Cronus/blob/master/doc/Configuration.md. Also, CronustContext does not like singletons due to multitenancy support.");
                return tenant;
            }
            set
            {
                tenant = value;
            }
        }

        public Dictionary<string, object> Trace { get; }

        public bool IsNotInitialized => string.IsNullOrEmpty(tenant) || ServiceProvider is null;

        public bool IsInitialized => IsNotInitialized == false;

        public IServiceProvider ServiceProvider { get; set; }
    }
}
