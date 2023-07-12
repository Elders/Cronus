using Elders.Cronus.MessageProcessing;
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
