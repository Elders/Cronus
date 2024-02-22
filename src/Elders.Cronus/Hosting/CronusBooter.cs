using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Elders.Cronus;

public sealed class CronusBooter
{
    private readonly IServiceProvider serviceProvider;

    public CronusBooter(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void BootstrapCronus()
    {
        var scanner = new CronusStartupScanner(new DefaulAssemblyScanner());
        IEnumerable<Type> startups = scanner.Scan();
        foreach (var startupType in startups)
        {
            ICronusStartup startup = (ICronusStartup)serviceProvider.GetRequiredService(startupType);
            startup.Bootstrap();
        }
    }
}
