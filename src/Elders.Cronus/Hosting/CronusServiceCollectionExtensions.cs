using System;
using System.Collections.Concurrent;
using System.Linq;
using Elders.Cronus.Discoveries;
using Elders.Cronus.Hosting;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Elders.Cronus
{
    public static class CronusServiceCollectionExtensions
    {
        /// <summary>
        /// // Adds Cronus core services
        /// </summary>
        public static IServiceCollection AddCronus(this IServiceCollection services, IConfiguration configuration)
        {
            return AddCronus(services, new CronusServicesProvider(services, configuration));
        }

        /// <summary>
        /// // Adds Cronus core services
        /// </summary>
        public static IServiceCollection AddCronus(this IServiceCollection services, CronusServicesProvider cronusServicesProvider)
        {
            services.AddTenantSupport();
            services.AddCronusHostOptions();
            services.AddDefaultSubscribers();

            var discoveryFinder = new DiscoveryScanner();
            var discoveryContext = new DiscoveryContext(AssemblyLoader.Assemblies.Values, cronusServicesProvider.Configuration);
            var discoveryResults = discoveryFinder.Scan(discoveryContext);

            foreach (var result in discoveryResults)
                cronusServicesProvider.HandleDiscoveredModel(result);

            return services;
        }

        internal static IServiceCollection AddTenantSupport(this IServiceCollection services)
        {
            services.AddTransient(typeof(SingletonPerTenant<>));
            services.AddSingleton(typeof(SingletonPerTenantContainer<>));
            services.AddScoped<CronusContext>();
            services.AddScoped<CronusContextFactory>();

            return services;
        }

        internal static IServiceCollection AddCronusHostOptions(this IServiceCollection services)
        {
            services.AddTransient<CronusHostOptions>();

            return services;
        }

        /// <summary>
        /// Adds a service which lifestyle is singleton per tenant
        /// </summary>
        public static IServiceCollection AddTenantSingleton<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddTransient<TImplementation>();
            services.AddTransient<TService>(provider => provider.GetRequiredService<SingletonPerTenant<TImplementation>>().Get());

            return services;
        }

        /// <summary>
        /// Replaces a service with a new one
        /// </summary>
        public static IServiceCollection Replace<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            var descriptorToRemove = services.FirstOrDefault(d => d.ServiceType == typeof(TService));
            if (descriptorToRemove is null)
            {
                throw new ArgumentException($"Service of type {typeof(TService).Name} is not registered and cannot be replaced.");
            }
            services.Remove(descriptorToRemove);
            var descriptorToAdd = new ServiceDescriptor(typeof(TService), typeof(TImplementation), descriptorToRemove.Lifetime);
            services.Add(descriptorToAdd);

            return services;
        }

        public static IServiceCollection AddOptions<TOptions, TOptionsProvider>(this IServiceCollection services)
            where TOptions : class, new()
            where TOptionsProvider : CronusOptionsProviderBase<TOptions>
        {
            services.AddSingleton<IConfigureOptions<TOptions>, TOptionsProvider>();
            services.AddSingleton<IOptionsChangeTokenSource<TOptions>, TOptionsProvider>();
            services.AddSingleton<IOptionsFactory<TOptions>, TOptionsProvider>();

            return services;
        }

    }

    public static class SubscriberCollectionServiceCollectionExtensions
    {
        public static IServiceCollection AddSubscribers<T>(this IServiceCollection services)
        {
            services.AddSingleton(typeof(ISubscriberCollection<T>), typeof(SubscriberCollection<T>));
            services.AddSingleton(typeof(ISubscriberFinder<T>), typeof(SubscriberFinder<T>));
            services.AddSingleton(typeof(ISubscriberWorkflow<T>), typeof(ScopedSubscriberWorkflow<T>));
            services.AddSingleton(typeof(ISubscriberFactory<T>), typeof(HandlerSubscriberFactory<T>));

            return services;
        }

        public static IServiceCollection AddDefaultSubscribers(this IServiceCollection services)
        {
            services.AddSubscribersWithOpenGenerics();

            services.AddApplicationServiceSubscribers();
            services.AddProjectionSubscribers();
            services.AddSubscribers<IPort>();
            services.AddSubscribers<IGateway>();
            services.AddSubscribers<ISaga>();

            return services;
        }

        public static IServiceCollection AddSubscribersWithOpenGenerics(this IServiceCollection services)
        {
            services.AddSingleton(typeof(ISubscriberCollection<>), typeof(SubscriberCollection<>));
            services.AddSingleton(typeof(ISubscriberFinder<>), typeof(SubscriberFinder<>));
            services.AddSingleton(typeof(ISubscriberWorkflow<>), typeof(ScopedSubscriberWorkflow<>));
            services.AddSingleton(typeof(ISubscriberFactory<>), typeof(HandlerSubscriberFactory<>));

            return services;
        }

        public static IServiceCollection AddApplicationServiceSubscribers(this IServiceCollection services)
        {
            services.AddSingleton(typeof(ISubscriberCollection<IApplicationService>), typeof(SubscriberCollection<IApplicationService>));
            services.AddSingleton(typeof(ISubscriberFinder<IApplicationService>), typeof(SubscriberFinder<IApplicationService>));
            services.AddSingleton(typeof(ISubscriberWorkflow<IApplicationService>), typeof(ApplicationServiceSubscriberWorkflow));
            services.AddSingleton(typeof(ISubscriberFactory<IApplicationService>), typeof(HandlerSubscriberFactory<IApplicationService>));

            return services;
        }

        public static IServiceCollection AddProjectionSubscribers(this IServiceCollection services)
        {
            services.AddSingleton(typeof(ISubscriberCollection<IProjection>), typeof(SubscriberCollection<IProjection>));
            services.AddSingleton(typeof(ISubscriberFinder<IProjection>), typeof(SubscriberFinder<IProjection>));
            services.AddSingleton(typeof(ISubscriberWorkflow<IProjection>), typeof(ProjectionSubscriberWorkflow));
            services.AddSingleton(typeof(ISubscriberFactory<IProjection>), typeof(HandlerSubscriberFactory<IProjection>));

            return services;
        }
    }

    public class SingletonPerTenantContainer<T> : IDisposable
    {
        public SingletonPerTenantContainer()
        {
            Stash = new ConcurrentDictionary<string, T>();
        }

        public ConcurrentDictionary<string, T> Stash { get; private set; }

        public void Dispose()
        {
            foreach (var item in Stash.Values)
            {
                if (item is IDisposable)
                    (item as IDisposable).Dispose();
            }
            Stash.Clear();
        }
    }

    // TODO: mynkow
    public class SingletonPerTenant<T>
    {
        private readonly SingletonPerTenantContainer<T> container;
        private readonly CronusContext context;

        public SingletonPerTenant(SingletonPerTenantContainer<T> container, CronusContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            this.container = container;
            this.context = context;
        }

        public T Get()
        {
            if (container.Stash.TryGetValue(context.Tenant, out T instance) == false)
            {
                instance = context.ServiceProvider.GetRequiredService<T>();
                container.Stash.TryAdd(context.Tenant, instance);
            }

            return instance;
        }
    }
}
