using System;
using System.Collections.Concurrent;

namespace Elders.Cronus.Projections.Versioning
{
    public class InMemoryProjectionVersionStore
    {
        private readonly ConcurrentDictionary<string, ProjectionVersions> store;

        public InMemoryProjectionVersionStore()
        {
            this.store = new ConcurrentDictionary<string, ProjectionVersions>();
        }

        public ProjectionVersions Get(string projectionName)
        {
            if (string.IsNullOrEmpty(projectionName)) throw new ArgumentNullException(nameof(projectionName));

            var versions = store.GetOrAdd(projectionName, new ProjectionVersions());
            return versions;
        }

        public void Cache(ProjectionVersion version)
        {
            if (ReferenceEquals(null, version)) throw new ArgumentNullException(nameof(version));
            var versions = store.GetOrAdd(version.ProjectionName, new ProjectionVersions());
            versions.Add(version);
        }
    }
}
