using Elders.Cronus.EventStore.Index;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace Elders.Cronus;

[CronusStartup(Bootstraps.EventStoreIndices)]
public class EventStoreIndicesStartup : ICronusStartup /// TODO: make this <see cref="ICronusTenantStartup"/>
{
    private TenantsOptions tenants;
    private CronusHostOptions cronusHostOptions;
    private readonly IPublisher<ICommand> publisher;
    private readonly TypeContainer<IEventStoreIndex> indexTypeContainer;
    private readonly ILogger<EventStoreIndicesStartup> logger;

    public EventStoreIndicesStartup(TypeContainer<IEventStoreIndex> indexTypeContainer, IOptionsMonitor<CronusHostOptions> cronusHostOptions, IOptionsMonitor<TenantsOptions> tenantsOptions, IPublisher<ICommand> publisher, ILogger<EventStoreIndicesStartup> logger)
    {
        this.tenants = tenantsOptions.CurrentValue;
        this.cronusHostOptions = cronusHostOptions.CurrentValue;
        this.publisher = publisher;
        this.logger = logger;
        this.indexTypeContainer = indexTypeContainer;

        cronusHostOptions.OnChange(CronusHostOptionsChanged);
        tenantsOptions.OnChange(OptionsChangedBootstrapEventStoreIndexForTenant);
    }

    public void Bootstrap()
    {
        if (cronusHostOptions.ApplicationServicesEnabled == false)
            return;

        foreach (var index in indexTypeContainer.Items)
        {
            foreach (var tenant in tenants.Tenants)
            {
                InitializeIndesForTenant(index, tenant);
            }
        }
    }

    private void InitializeIndesForTenant(Type index, string tenant)
    {
        if (cronusHostOptions.ApplicationServicesEnabled == false)
            return;

        var id = new EventStoreIndexManagerId(index.GetContractId(), tenant);
        var command = new RegisterIndex(id);
        publisher.Publish(command);
    }

    private void OptionsChangedBootstrapEventStoreIndexForTenant(TenantsOptions newOptions)
    {
        if (tenants.Tenants.SequenceEqual(newOptions.Tenants) == false) // Check for difference between tenants and newOptions
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Cronus tenants options re-loaded with {@options}", newOptions);

            // Find the difference between the old and new tenants
            // and bootstrap the new tenants
            var newTenants = newOptions.Tenants.Except(tenants.Tenants);
            foreach (var index in indexTypeContainer.Items)
            {
                foreach (var tenant in newTenants)
                {
                    InitializeIndesForTenant(index, tenant);
                }
            }

            tenants = newOptions;
        }
    }

    private void CronusHostOptionsChanged(CronusHostOptions newOptions)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Cronus host options re-loaded with {@options}", newOptions);

        cronusHostOptions = newOptions;
    }
}
