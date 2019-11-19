﻿using System;
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

            Type discoveryResultType = discoveryResult.GetType();
            Type genericArgumentType = discoveryResultType.GetGenericArguments().Single();
            MethodInfo handler = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(m => m.Name.Equals("Handle", StringComparison.OrdinalIgnoreCase))
                .Where(m => m.GetParameters().Any(p => p.ParameterType.GetGenericArguments().Single() == genericArgumentType))
                .SingleOrDefault();

            if (handler is null)
                throw new RuntimeBinderException($"Missing handle for IDiscoveryResult<{genericArgumentType.Name}>");

            handler.Invoke(this, new[] { discoveryResult });
        }

        protected void AddServices(IDiscoveryResult<object> discoveryResult)
        {
            discoveryResult.AddServices(Services);

            foreach (var discoveredModel in discoveryResult.Models)
            {
                if (discoveredModel.CanOverrideDefaults)
                {
                    Services.Replace(discoveredModel);
                }
                else
                {
                    Services.TryAdd(discoveredModel);
                }
            }
        }

        protected virtual void Handle(DiscoveryResult<ProjectionPlayer> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<ICronusHost> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<ISerializer> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IConsumer<object>> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IPublisher<IMessage>> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IProjectionReader> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IProjectionWriter> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IAggregateRootAtomicAction> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IApplicationService> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IProjection> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IPort> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<ISaga> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IGateway> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IWorkflow> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IHandlerFactory> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IEventStore> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IAggregateRepository> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IMultitenancy> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<MigrationDiscovery> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<ICronusJob<object>> discoveryResult) => AddServices(discoveryResult);
    }
}
