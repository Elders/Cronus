//using System;
//using System.Collections.Generic;
//using System.Text;
//using Elders.Cronus.Projections;
//using Elders.Cronus.Projections.Cassandra.EventSourcing;

//[assembly: Elders.Cronus.BoundedContext("Elders.Cronus.Cluster")]

//namespace Elders.Cronus.Cluster
//{
//    public interface IClusterService
//    {

//    }

//    public class ClusterHandler : IClusterService,
//        IEventHandler<ReplayProjectionStarted>,
//        IEventHandler<ReplayProjectionFinished>,
//        IEventHandler<ReplayProjectionCanceled>
//    {
//        public IProjectionVersionStore ProjectionVersionStore { get; set; }

//        public void Handle(ReplayProjectionCanceled @event)
//        {
//            ProjectionVersionStore.Store(@event.ProjectionVersion);
//        }

//        public void Handle(ReplayProjectionStarted @event)
//        {
//            ProjectionVersionStore.Store(@event.ProjectionVersion);
//        }

//        public void Handle(ReplayProjectionFinished @event)
//        {
//            ProjectionVersionStore.Store(@event.ProjectionVersion);
//        }
//    }

//    public interface IProjectionVersionStore
//    {
//        ProjectionVersions Get(string projectionName);

//        void Store(ProjectionVersion version);
//    }

//    public class InMemoryProjectionVersionStore : IProjectionVersionStore
//    {
//        private readonly Dictionary<string, ProjectionVersions> store;

//        public InMemoryProjectionVersionStore()
//        {
//            this.store = new Dictionary<string, ProjectionVersions>();
//        }

//        public ProjectionVersions Get(string projectionName)
//        {
//            ProjectionVersions versions;
//            if (store.TryGetValue(projectionName, out versions) == false)
//            {
//                versions = new ProjectionVersions();
//                versions.Add(new ProjectionVersion(projectionName, ProjectionStatus.Live, 1));
//            }
//            return versions;
//        }

//        public void Store(ProjectionVersion version)
//        {
//            ProjectionVersions versions;
//            if (store.TryGetValue(version.ProjectionContractId, out versions))
//            {
//                versions.Add(version);
//            }
//            else
//            {
//                var initi = new ProjectionVersions();
//                initi.Add(version);
//                store.Add(version.ProjectionContractId, initi);
//            }
//        }
//    }

//    //public class InMemoryProjectionVersionResolver : IProjectionVersionResolver
//    //{
//    //    readonly IProjectionVersionStore store;

//    //    public InMemoryProjectionVersionResolver(IProjectionVersionStore versionStore)
//    //    {
//    //        store = versionStore;
//    //    }

//    //    public ProjectionVersions GetVersions(Type projectionType)
//    //    {
//    //        var contractId = projectionType.GetContractId();
//    //        return GetVersions(contractId);
//    //    }

//    //    public ProjectionVersions GetVersions(string projectionName)
//    //    {
//    //        return store.Get(projectionName);
//    //    }
//    //}
//}
