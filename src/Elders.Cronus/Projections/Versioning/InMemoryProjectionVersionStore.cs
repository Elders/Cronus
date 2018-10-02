﻿using System;
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

            ProjectionVersions versions;
            if (store.TryGetValue(projectionName, out versions))
                return versions;

            return new ProjectionVersions();
        }

        public void Cache(ProjectionVersion version)
        {
            if (ReferenceEquals(null, version)) throw new ArgumentNullException(nameof(version));

            ProjectionVersions versions;
            if (store.TryGetValue(version.ProjectionName, out versions))
            {
                versions.Add(version);
            }
            else
            {
                var initialVersion = new ProjectionVersions();
                initialVersion.Add(version);
                store.AddOrUpdate(version.ProjectionName, initialVersion, (key, val) => val);
            }
        }
    }
}
