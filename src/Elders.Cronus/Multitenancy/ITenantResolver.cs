using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Elders.Cronus.AspNetCore.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.Multitenancy;

public interface ITenantResolver
{
    string Resolve(object source);
}

public interface ITenantResolver<in T>
{
    /// <summary>
    /// Resolves a teannt name from a specific source.
    /// </summary>
    /// <param name="source">The source from which the system will try to extract the tenant name.</param>
    /// <returns>Returns the tenant name or empty string if the tenant resolving fails.</returns>
    /// <remarks>
    /// This method should never throw an exception. The system will try to resolve the tenant name
    /// from several places based on the current execution chain. If the process still fails then
    /// an exception will be thrown from the underlying execution.
    /// </remarks>
    string Resolve(T source);
}

public class TenantResolver : ITenantResolver
{
    private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(TenantResolver));
    ConcurrentDictionary<Type, ResolverCache> resolvers = new ConcurrentDictionary<Type, ResolverCache>();

    private TenantsOptions tenants;
    private readonly IServiceProvider serviceProvider;

    public TenantResolver(IServiceProvider serviceProvider, IOptionsMonitor<TenantsOptions> tenantsOptions)
    {
        this.serviceProvider = serviceProvider;
        this.tenants = tenantsOptions.CurrentValue;

        tenantsOptions.OnChange(OnTenantsOptionsChanged);
    }

    public string Resolve(object source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        ResolverCache resolverCache;
        if (resolvers.TryGetValue(source.GetType(), out resolverCache) == false)
        {
            Type tenantResolverType = typeof(ITenantResolver<>);
            Type tenantResolverTypeGeneric = tenantResolverType.MakeGenericType(source.GetType());
            object resolver = serviceProvider.GetRequiredService(tenantResolverTypeGeneric);
            MethodInfo resolveMethod = tenantResolverTypeGeneric.GetMethod("Resolve");

            resolverCache = new ResolverCache(resolver, resolveMethod);
            resolvers.TryAdd(source.GetType(), resolverCache);
        }

        string tenant = resolverCache.GetTenantFrom(source);

        if (string.IsNullOrEmpty(tenant) == false)
        {
            return tenant;
        }
        else
        {
            if (tenants.Tenants.Count() == 1)
                return tenants.Tenants.Single();
        }

        throw new UnableToResolveTenantException("Unable to resolve tenant.");
    }

    private void OnTenantsOptionsChanged(TenantsOptions newOptions)
    {
        logger.Info(() => "Cronus tenants options re-loaded with {@options}", newOptions);

        tenants = newOptions;
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
            return _resolveMethod.Invoke(_resolver, new object[] { source })?.ToString();
        }
    }
}
