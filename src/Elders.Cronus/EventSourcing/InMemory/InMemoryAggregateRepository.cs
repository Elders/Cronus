using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.DomainModeling;
using System.Collections.Concurrent;

namespace Elders.Cronus.EventSourcing.InMemory
{
    public class InMemoryAggregateRepository : IAggregateRepository
    {
        private static AggregateVersionService versionService = new AggregateVersionService();
        private IEventStorePersister persister;

        public InMemoryAggregateRepository(IEventStorePersister persister)
        {
            this.persister = persister;
        }

        public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            if (aggregateRoot.UncommittedEvents == null || aggregateRoot.UncommittedEvents.Count == 0)
                return;
            aggregateRoot.State.Version += 1;

            int reservedVersion = versionService.ReserveVersion(aggregateRoot.State.Id, aggregateRoot.State.Version);
            if (reservedVersion != aggregateRoot.State.Version)
            {
                throw new Exception("Retry command");
            }
            persister.Persist(new List<IAggregateRoot>() { aggregateRoot });
            aggregateRoot.UncommittedEvents.Clear();
        }

        public AR Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            if (!InMemoryEventStoreStorage.EventsStreams.ContainsKey(id))
                return default(AR);
            else
            {
                var evnts = new ConcurrentQueue<IEvent>();

                var streams = InMemoryEventStoreStorage.EventsStreams[id];

                AR aggregateRoot = AggregateRootFactory.Build<AR>(streams.SelectMany(x => x.Events).ToList());
                aggregateRoot.State.Version = streams.Last().Version;
                return aggregateRoot;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}