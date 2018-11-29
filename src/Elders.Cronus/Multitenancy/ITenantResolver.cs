using Elders.Cronus.EventStore;
using Elders.Cronus.Projections;

namespace Elders.Cronus.Multitenancy
{
    public interface ITenantResolver
    {
        string Resolve(IAggregateRootId id);

        string Resolve(AggregateCommit aggregateCommit);

        string Resolve(ProjectionCommit projectionCommit);

        string Resolve(IBlobId id);

        string Resolve(IMessage message);

        string Resolve(CronusMessage cronusMessage);
    }
}
