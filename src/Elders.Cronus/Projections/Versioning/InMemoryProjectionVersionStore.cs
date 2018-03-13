using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Elders.Cronus.Projections.Versioning
{
    public class InMemoryProjectionVersionStore
    {
        private readonly ConcurrentDictionary<string, ProjectionVersions> store;

        public InMemoryProjectionVersionStore()
        {
            this.store = new ConcurrentDictionary<string, ProjectionVersions>();
        }

        public ProjectionVersions Get(string projectionContractId)
        {
            ProjectionVersions versions;
            if (store.TryGetValue(projectionContractId, out versions))
                return versions;

            return new ProjectionVersions();
        }

        public void Cache(ProjectionVersion version)
        {
            ProjectionVersions versions;
            if (store.TryGetValue(version.ProjectionContractId, out versions))
            {
                versions.Add(version);
            }
            else
            {
                var initialVersion = new ProjectionVersions();
                initialVersion.Add(version);
                store.AddOrUpdate(version.ProjectionContractId, initialVersion, (key, val) => val);
            }
        }
    }
}
