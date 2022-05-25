using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Elders.Cronus.Discoveries
{
    public class CronusServicesProvider : ICronusServicesProvider
    {
        public CronusServicesProvider(IServiceCollection services, IConfiguration configuration)
        {
            Services = services;
            Configuration = configuration;
        }

        public IServiceCollection Services { get; }

        public IConfiguration Configuration { get; }

        public void HandleDiscoveredModel(IDiscoveryResult<object> discoveryResult)
        {
            if (discoveryResult is null) throw new ArgumentNullException(nameof(discoveryResult));

            AddServices(discoveryResult);
        }

        protected void AddServices(IDiscoveryResult<object> discoveryResult)
        {
            discoveryResult.AddServices(Services);

            foreach (var discoveredModel in discoveryResult.Models)
            {
                if (discoveredModel.CanAddMultiple)
                {
                    Services.Add(discoveredModel);
                }
                else if (discoveredModel.CanOverrideDefaults)
                {
                    Services.Replace(discoveredModel);
                }
                else
                {
                    Services.TryAdd(discoveredModel);
                }
            }
        }
    }
}
