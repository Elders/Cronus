//using System;
//using System.Collections.Concurrent;

//namespace Elders.Cronus.Projections.Versioning
//{
//    public class InMemoryProjectionVersionStore
//    {
//        private readonly ConcurrentDictionary<string, ProjectionVersions> store;

//        public InMemoryProjectionVersionStore()
//        {
//            this.store = new ConcurrentDictionary<string, ProjectionVersions>();
//        }

//        public ProjectionVersions Get(string projectionName)
//        {
//            if (string.IsNullOrEmpty(projectionName)) throw new ArgumentNullException(nameof(projectionName));

//            return store.GetOrAdd(projectionName, new ProjectionVersions());
//        }

//        public void Cache(ProjectionVersion version)
//        {
//            if (version is null) throw new ArgumentNullException(nameof(version));

//            ProjectionVersions versions = Get(version.ProjectionName);
//            lock (versions)
//            {
//                versions.Add(version);
//            }
//        }
//    }
//}
