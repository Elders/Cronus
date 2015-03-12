using System.Collections.Concurrent;

namespace Elders.Cronus.DomainModeling
{
    public class InMemoryAggregateRevisionService : IAggregateRevisionService
    {
        private static ConcurrentDictionary<IAggregateRootId, int> reservedRevisions = new ConcurrentDictionary<IAggregateRootId, int>();


        /// <summary>
        /// Reserves the revision.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="requestedVersion">The requested version.</param>
        /// <returns></returns>
        public int ReserveRevision(IAggregateRootId aggregateId, int requestedVersion)
        {
            var reservedVersion = reservedRevisions.AddOrUpdate(aggregateId, requestedVersion, (ar, current) => ++current);
            if (requestedVersion == reservedVersion)
                return reservedVersion;
            else
                return reservedVersion - 1;
        }

        public void Dispose()
        {
            if (reservedRevisions != null)
                reservedRevisions = null;
        }
    }
}