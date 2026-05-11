using System;

namespace Elders.Cronus.Projections;

/// <summary>
/// Memoizes the discovery-time <see cref="ProjectionVersions"/> synthesized by
/// <see cref="ProjectionRepository"/> when the canonical <c>ProjectionVersionsHandler</c>
/// stream is empty for a given <c>(projectionName, tenant)</c>. Every <c>GetOrAdd</c>
/// returns a fresh clone, so callers cannot mutate the cached entry.
/// </summary>
public interface IDiscoveryTimeVersionsCache
{
    /// <summary>
    /// Returns a clone of the cached <see cref="ProjectionVersions"/> for
    /// <paramref name="projectionName"/> and <paramref name="tenant"/>. On first call invokes
    /// <paramref name="factory"/> and stores the result; subsequent calls return clones of
    /// the stored value. If the factory returns <c>null</c>, nothing is cached and
    /// <c>null</c> is returned.
    /// </summary>
    /// <param name="projectionName">The projection contract name.</param>
    /// <param name="tenant">The Cronus tenant the cache entry belongs to.</param>
    /// <param name="factory">Builds the canonical <see cref="ProjectionVersions"/> on cache miss.</param>
    /// <returns>A fresh clone of the cached versions, or <c>null</c> when the factory returned <c>null</c>.</returns>
    ProjectionVersions GetOrAdd(string projectionName, string tenant, Func<ProjectionVersions> factory);
}
