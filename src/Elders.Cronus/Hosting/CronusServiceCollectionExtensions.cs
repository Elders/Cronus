using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Elders.Cronus.Diagnostics;
using Elders.Cronus.Discoveries;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;
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
            services.AddOpenTelemetry();
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

        internal static IServiceCollection AddOpenTelemetry(this IServiceCollection services)
        {
            // https://github.com/dotnet/aspnetcore/blob/f3f9a1cdbcd06b298035b523732b9f45b1408461/src/Hosting/Hosting/src/WebHostBuilder.cs#L334
            // By default aspnet core registers a DiagnosticListener and if we add our own you will loose the http insights
            // However, for worker services we need to register our own Listener.
            // I am sure there is a better way for doing the setup so please if you end up here and you have problems please fix it.
            if (services.Any(x => x.ServiceType == typeof(DiagnosticListener)) == false)
            {
                var listener = new DiagnosticListener(CronusDiagnostics.Name);
                services.AddSingleton<DiagnosticListener>(listener);
                services.AddSingleton<DiagnosticSource>(listener);
            }

            return services;
        }

        internal static IServiceCollection AddTenantSupport(this IServiceCollection services)
        {
            services.AddOptions<TenantsOptions, TenantsOptionsProvider>();
            services.AddTransient(typeof(SingletonPerTenant<>));
            services.AddSingleton(typeof(SingletonPerTenantContainer<>));
            services.AddScoped<CronusContext>();
            services.AddScoped<CronusContextFactory>();

            return services;
        }

        internal static IServiceCollection AddCronusHostOptions(this IServiceCollection services)
        {
            services.AddOptions();
            services.AddOptions<CronusHostOptions, CronusHostOptionsProvider>();
            services.AddOptions<BoundedContext, BoundedContextProvider>();

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
            services.AddSingleton(typeof(ISubscriberWorkflowFactory<T>), typeof(ScopedSubscriberWorkflow<T>));
            services.AddSingleton(typeof(ISubscriberFactory<T>), typeof(HandlerSubscriberFactory<T>));

            return services;
        }

        public static IServiceCollection AddDefaultSubscribers(this IServiceCollection services)
        {
            services.AddSubscribersWithOpenGenerics();

            services.AddApplicationServiceSubscribers();
            services.AddSubscribers<IPort>();
            services.AddSubscribers<IGateway>();
            services.AddSubscribers<ISaga>();
            services.AddSubscribers<ITrigger>();
            services.AddEventStoreIndexSubscribers();

            return services;
        }

        public static IServiceCollection AddSubscribersWithOpenGenerics(this IServiceCollection services)
        {
            services.AddSingleton(typeof(ISubscriberCollection<>), typeof(SubscriberCollection<>));
            services.AddSingleton(typeof(ISubscriberFinder<>), typeof(SubscriberFinder<>));
            services.AddSingleton(typeof(ISubscriberWorkflowFactory<>), typeof(ScopedSubscriberWorkflow<>));
            services.AddSingleton(typeof(ISubscriberFactory<>), typeof(HandlerSubscriberFactory<>));

            return services;
        }

        public static IServiceCollection AddApplicationServiceSubscribers(this IServiceCollection services)
        {
            services.AddSingleton(typeof(ISubscriberCollection<IApplicationService>), typeof(SubscriberCollection<IApplicationService>));
            services.AddSingleton(typeof(ISubscriberFinder<IApplicationService>), typeof(SubscriberFinder<IApplicationService>));
            services.AddSingleton(typeof(ISubscriberWorkflowFactory<IApplicationService>), typeof(ApplicationServiceSubscriberWorkflow));
            services.AddSingleton(typeof(ISubscriberFactory<IApplicationService>), typeof(HandlerSubscriberFactory<IApplicationService>));

            return services;
        }

        public static IServiceCollection AddEventStoreIndexSubscribers(this IServiceCollection services)
        {
            services.AddSingleton(typeof(ISubscriberCollection<IEventStoreIndex>), typeof(SubscriberCollection<IEventStoreIndex>));
            services.AddSingleton(typeof(ISubscriberFinder<IEventStoreIndex>), typeof(SubscriberFinder<IEventStoreIndex>));
            services.AddSingleton(typeof(ISubscriberWorkflowFactory<IEventStoreIndex>), typeof(EventStoreIndexSubscriberWorkflow));
            services.AddSingleton(typeof(ISubscriberFactory<IEventStoreIndex>), typeof(EventStoreIndexSubscriberFactory));

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
