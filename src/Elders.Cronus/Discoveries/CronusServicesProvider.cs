using System;
using System.Linq;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.EventStore;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Workflow;
using Elders.Cronus.Multitenancy;
using Elders.Cronus.Projections;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Elders.Cronus.Cluster.Job;

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
