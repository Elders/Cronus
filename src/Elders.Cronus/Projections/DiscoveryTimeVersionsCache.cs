using System;
using System.Collections.Concurrent;

namespace Elders.Cronus.Projections;

/// <summary>
/// Singleton-scoped in-memory implementation of <see cref="IDiscoveryTimeVersionsCache"/>.
/// Each entry is keyed by <c>(projectionName, tenant)</c> and lives for the lifetime of the
/// host. The cache is bypassed in production once
/// <c>ProjectionRepository.GetProjectionVersionsFromStoreAsync</c> returns <c>IsSuccess=true</c>,
/// so stale entries are never observed.
/// </summary>
public sealed class DiscoveryTimeVersionsCache : IDiscoveryTimeVersionsCache
{
    private readonly ConcurrentDictionary<(string ProjectionName, string Tenant), ProjectionVersions> entries
        = new ConcurrentDictionary<(string ProjectionName, string Tenant), ProjectionVersions>();

    public ProjectionVersions GetOrAdd(string projectionName, string tenant, Func<ProjectionVersions> factory)
    {
        ArgumentNullException.ThrowIfNull(projectionName);
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(factory);

        var key = (projectionName, tenant);

        if (entries.TryGetValue(key, out ProjectionVersions cached) == false)
        {
            cached = factory();
            if (cached is null)
                return null;

            entries.TryAdd(key, cached);
        }

        return new ProjectionVersions([.. cached]);
    }
}
