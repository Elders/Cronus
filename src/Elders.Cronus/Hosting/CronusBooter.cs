using Elders.Cronus.Discoveries;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Elders.Cronus.Hosting
{
    public class CronusBooter
    {
        public static void BootstrapCronus(IServiceProvider serviceProvider)
        {
            var scanner = new CronusStartupScanner();
            IEnumerable<Type> startups = scanner.Scan();
            foreach (var startupType in startups)
            {
                ICronusStartup startup = (ICronusStartup)serviceProvider.GetRequiredService(startupType);
                startup.Bootstrap();
            }
        }
    }
}
