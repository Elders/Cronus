using System;
using System.Collections.Generic;

namespace Elders.Cronus.MessageProcessing
{
    public sealed class CronusContext
    {
        public CronusContext(string tenant, IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(tenant)) throw new ArgumentException($"Unknown tenant. CronusContext is not properly built. Make sure that you have properly configured `cronus:tenants`. More info at https://github.com/Elders/Cronus/blob/master/doc/Configuration.md");
            if (serviceProvider is null) throw new ArgumentNullException(nameof(serviceProvider));

            Tenant = tenant;
            ServiceProvider = serviceProvider;
            Trace = new Dictionary<string, object>();
        }

        public string Tenant { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public Dictionary<string, object> Trace { get; }

        public bool IsNotInitialized => string.IsNullOrEmpty(Tenant) || ServiceProvider is null;

        public bool IsInitialized => IsNotInitialized == false;
    }
}
