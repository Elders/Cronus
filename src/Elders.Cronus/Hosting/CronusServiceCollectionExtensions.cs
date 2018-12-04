using System;
using System.Collections.Concurrent;
using System.Linq;
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
            return AddCronus(services, new CronusServicesProvider(services, configuration));
        }

        public static IServiceCollection AddCronus(this IServiceCollection services, CronusServicesProvider cronusServicesProvider)
        {
            services.AddTenantSupport();
            services.AddCronusHostOptions();

            var discoveryFinder = new DiscoveryScanner(cronusServicesProvider);
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

        internal static IServiceCollection AddCronusHostOptions(this IServiceCollection services)
        {
            services.AddTransient<CronusHostOptions>();

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
