using System;

namespace Elders.Cronus.EventStore
{
    public class MultiTenantEventStore : IEventStore
    {
        readonly IEventStoreFactory factory;

        readonly ITenantResolver tenantResolver;

        public MultiTenantEventStore(IEventStoreFactory factory, ITenantResolver tenantResolver)
        {
            if (ReferenceEquals(null, factory) == true) throw new ArgumentNullException(nameof(factory));
            if (ReferenceEquals(null, tenantResolver) == true) throw new ArgumentNullException(nameof(tenantResolver));

            this.factory = factory;
            this.tenantResolver = tenantResolver;
        }

        public void Append(AggregateCommit aggregateCommit)
        {
            if (ReferenceEquals(null, aggregateCommit) == true) throw new ArgumentNullException(nameof(aggregateCommit));

            var tenant = tenantResolver.Resolve(aggregateCommit);
            var store = factory.GetEventStore(tenant);
            store.Append(aggregateCommit);
        }

        public EventStream Load(IAggregateRootId aggregateId)
        {
            if (ReferenceEquals(null, aggregateId) == true) throw new ArgumentNullException(nameof(aggregateId));

            var tenant = tenantResolver.Resolve(aggregateId);
            var store = factory.GetEventStore(tenant);
            return store.Load(aggregateId);
        }

        public EventStream Load(IAggregateRootId aggregateId, Func<IAggregateRootId, string> getBoundedContext = null)
        {
            if (ReferenceEquals(null, aggregateId) == true) throw new ArgumentNullException(nameof(aggregateId));

            var tenant = tenantResolver.Resolve(aggregateId);
            var store = factory.GetEventStore(tenant);
            return store.Load(aggregateId, getBoundedContext);
        }
    }
}
