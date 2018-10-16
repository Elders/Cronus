using Elders.Cronus.Multitenancy;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections
{
    public class MultiTenantProjectionStore : IProjectionStore
    {
        readonly IProjectionStoreFactory factory;
        readonly ITenantResolver tenantResolver;

        public MultiTenantProjectionStore(IProjectionStoreFactory factory, ITenantResolver tenantResolver)
        {
            if (ReferenceEquals(null, factory) == true) throw new ArgumentNullException(nameof(factory));
            if (ReferenceEquals(null, tenantResolver) == true) throw new ArgumentNullException(nameof(tenantResolver));

            this.factory = factory;
            this.tenantResolver = tenantResolver;
        }

        public IEnumerable<ProjectionCommit> EnumerateProjection(ProjectionVersion version, IBlobId projectionId)
        {
            if (ReferenceEquals(null, version) == true) throw new ArgumentNullException(nameof(version));
            if (ReferenceEquals(null, projectionId) == true) throw new ArgumentNullException(nameof(projectionId));

            var tenant = tenantResolver.Resolve(projectionId);
            var store = factory.GetProjectionStore(tenant);

            return store.EnumerateProjection(version, projectionId);
        }

        public IEnumerable<ProjectionCommit> Load(ProjectionVersion version, IBlobId projectionId, int snapshotMarker)
        {
            if (ReferenceEquals(null, version) == true) throw new ArgumentNullException(nameof(version));
            if (ReferenceEquals(null, projectionId) == true) throw new ArgumentNullException(nameof(projectionId));
            if (ReferenceEquals(null, snapshotMarker) == true) throw new ArgumentNullException(nameof(snapshotMarker));

            var tenant = tenantResolver.Resolve(projectionId);
            var store = factory.GetProjectionStore(tenant);

            return store.Load(version, projectionId, snapshotMarker);
        }

        public Task<IEnumerable<ProjectionCommit>> LoadAsync(ProjectionVersion version, IBlobId projectionId, int snapshotMarker)
        {
            if (ReferenceEquals(null, version) == true) throw new ArgumentNullException(nameof(version));
            if (ReferenceEquals(null, projectionId) == true) throw new ArgumentNullException(nameof(projectionId));
            if (ReferenceEquals(null, snapshotMarker) == true) throw new ArgumentNullException(nameof(snapshotMarker));

            var tenant = tenantResolver.Resolve(projectionId);
            var store = factory.GetProjectionStore(tenant);
            return store.LoadAsync(version, projectionId, snapshotMarker);
        }

        public void Save(ProjectionCommit commit)
        {
            if (ReferenceEquals(null, commit) == true) throw new ArgumentNullException(nameof(commit));

            var tenant = tenantResolver.Resolve(commit);
            var store = factory.GetProjectionStore(tenant);
            store.Save(commit);
        }
    }


    public interface IProjectionStoreFactory
    {
        IProjectionStore GetProjectionStore(string tenant);
        IEnumerable<IProjectionStore> GetProjectionStores();
    }
}
