using System.Collections.Concurrent;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Persistence.Cassandra
{
    public class AggregateVersionService
    {
        ConcurrentDictionary<IAggregateRootId, int> reservedRevisions = new ConcurrentDictionary<IAggregateRootId, int>();

        public int ReserveVersion(IAggregateRootId aggregateId, int requestedVersion)
        {
            var reservedVersion = reservedRevisions.AddOrUpdate(aggregateId, requestedVersion, (ar, current) => ++current);
            if (requestedVersion == reservedVersion)
                return reservedVersion;
            else
                return reservedVersion - 1;
        }
    }
}