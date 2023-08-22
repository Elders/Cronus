using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace Elders.Cronus
{
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
                if (item is IDisposable disposableItem)
                    disposableItem.Dispose();
            }
            Stash.Clear();
        }
    }

    // TODO: mynkow
    public class SingletonPerTenant<T>
    {
        private readonly SingletonPerTenantContainer<T> container;
        private readonly ICronusContextAccessor contextAccessor;

        public SingletonPerTenant(SingletonPerTenantContainer<T> container, ICronusContextAccessor contextAccessor)
        {
            if (contextAccessor is null) throw new ArgumentNullException(nameof(contextAccessor));
            this.container = container;
            this.contextAccessor = contextAccessor;
        }

        public T Get()
        {
            if (container.Stash.TryGetValue(contextAccessor.CronusContext.Tenant, out T instance) == false)
            {
                instance = contextAccessor.CronusContext.ServiceProvider.GetRequiredService<T>();
                instance.AssignPropertySafely<IHaveTenant>(x => x.Tenant = contextAccessor.CronusContext.Tenant);
                container.Stash.TryAdd(contextAccessor.CronusContext.Tenant, instance);
            }

            return instance;
        }
    }
}
