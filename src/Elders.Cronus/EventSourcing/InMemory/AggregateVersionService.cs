using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.DomainModeling;
using System.Collections.Concurrent;

namespace Elders.Cronus.EventSourcing.InMemory
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