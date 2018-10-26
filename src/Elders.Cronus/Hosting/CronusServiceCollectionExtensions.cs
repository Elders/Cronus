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
            services.AddSingleton(typeof(SingletonPerTenant<>));
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

    // TODO: mynkow
    public class SingletonPerTenant<T> : IDisposable
    {
        static ConcurrentDictionary<string, T> cache = new ConcurrentDictionary<string, T>();

        private readonly CronusContext context;
        private readonly IServiceProvider provider;

        public SingletonPerTenant(IServiceProvider provider)
        {
            if (provider is null) throw new ArgumentNullException(nameof(provider));

            this.provider = provider;
        }

        public T Get()
        {
            CronusContext context = provider.GetRequiredService<CronusContext>();

            if (cache.TryGetValue(context.Tenant, out T instance) == false)
            {
                instance = provider.GetRequiredService<T>();
                cache.TryAdd(context.Tenant, instance);
            }

            return instance;
        }

        public void Dispose()
        {
            foreach (var item in cache.Values)
            {
                if (item is IDisposable)
                    (item as IDisposable).Dispose();
            }
            cache.Clear();
        }
    }
}
