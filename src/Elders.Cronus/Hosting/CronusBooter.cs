using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Elders.Cronus;

public sealed class CronusBooter
{
    private readonly IServiceProvider serviceProvider;
    private TenantsOptions tenantOptions;
    private readonly ILogger<CronusBooter> logger;

    public CronusBooter(IServiceProvider serviceProvider, IOptionsMonitor<TenantsOptions> monitor, ILogger<CronusBooter> logger)
    {
        this.serviceProvider = serviceProvider;
        this.tenantOptions = monitor.CurrentValue;
        this.logger = logger;

        monitor.OnChange(OnTenantsOptionsChanged);
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

        IEnumerable<Type> tenantStartups = scanner.ScanForCronusTenantStartups();
        foreach (var tenantStartupType in tenantStartups)
        {
            foreach (string tenant in tenantOptions.Tenants)
            {
                using (var scopedServiceProvider = serviceProvider.CreateScope())
                {
                    var cronusContextFactory = scopedServiceProvider.ServiceProvider.GetRequiredService<DefaultCronusContextFactory>();
                    var cronusContext = cronusContextFactory.Create(tenant, scopedServiceProvider.ServiceProvider);

                    ICronusTenantStartup tenantStartUp = (ICronusTenantStartup)cronusContext.ServiceProvider.GetRequiredService(tenantStartupType);
                    tenantStartUp.Bootstrap();
                }
            }
        }
    }

    private void OnTenantsOptionsChanged(TenantsOptions newOptions)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Cronus tenants options re-loaded with {@options}", newOptions);

        tenantOptions = newOptions;
    }
}
