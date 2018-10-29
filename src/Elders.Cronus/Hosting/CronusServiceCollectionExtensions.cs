using System;
using System.Collections.Concurrent;
using Elders.Cronus.Discoveries;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus
{
    public static class CronusServiceCollectionExtensions
    {
        public static IServiceCollection AddCronus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTenantSupport();

            var discoveryFinder = new DiscoveryScanner(new CronusServicesProvider(services), configuration);
            discoveryFinder.Discover();

            return services;
        }

        internal static IServiceCollection AddTenantSupport(this IServiceCollection services)
        {
            services.AddTransient(typeof(SingletonPerTenant<>));
            services.AddSingleton(typeof(SingletonPerTenantContainer<>));
            services.AddScoped<CronusContext>();

            return services;
        }

        public static IServiceCollection AddTenantSingleton<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddTransient<TImplementation>();
            services.AddTransient<TService>(provider => provider.GetRequiredService<SingletonPerTenant<TImplementation>>().Get());

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
