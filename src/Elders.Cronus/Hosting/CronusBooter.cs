using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Elders.Cronus
{
    public sealed class CronusBooter
    {
        private readonly IServiceProvider serviceProvider;

        public CronusBooter(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void BootstrapCronus()
        {
            CronusLogger.Configure(serviceProvider.GetService<ILoggerFactory>());
            var scanner = new CronusStartupScanner(new DefaulAssemblyScanner());
            IEnumerable<Type> startups = scanner.Scan();
            foreach (var startupType in startups)
            {
                ICronusStartup startup = (ICronusStartup)serviceProvider.GetRequiredService(startupType);
                startup.Bootstrap();
            }
        }
    }
}
