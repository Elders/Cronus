using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Elders.Cronus.Multitenancy
{
    public interface ITenantResolver
    {
        string Resolve(object source);
    }

    public interface ITenantResolver<in T>
    {
        string Resolve(T source);
    }

    public class TenantResolver : ITenantResolver
    {
        Dictionary<Type, ResolverCache> resolvers = new Dictionary<Type, ResolverCache>();

        private readonly IServiceProvider serviceProvider;

        public TenantResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public string Resolve(object source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            ResolverCache resolverCache;
            if (resolvers.TryGetValue(source.GetType(), out resolverCache) == false)
            {
                var tenantResolverType = typeof(ITenantResolver<>);
                var tenantResolverTypeGeneric = tenantResolverType.MakeGenericType(source.GetType());
                var resolver = serviceProvider.GetRequiredService(tenantResolverTypeGeneric);
                MethodInfo resolveMethod = tenantResolverTypeGeneric.GetMethod("Resolve");

                resolverCache = new ResolverCache(resolver, resolveMethod);
                resolvers.TryAdd(source.GetType(), resolverCache);
            }

            return resolverCache.GetTenantFrom(source);
        }

        class ResolverCache
        {
            private readonly object _resolver;
            private readonly MethodInfo _resolveMethod;

            public ResolverCache(object resolver, MethodInfo resolveMethod)
            {
                _resolver = resolver;
                _resolveMethod = resolveMethod;
            }

            public string GetTenantFrom(object source)
            {
                return _resolveMethod.Invoke(_resolver, new object[] { source }).ToString();
            }
        }
    }
}
