using System;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;
using NMSD.Protoreg;

namespace NMSD.Cronus.EventSourcing
{
    public class RabbitRepository : IAggregateRepository
    {
        private MssqlEventStore mssqlStore;

        private IPublisher<DomainMessageCommit> eventPublisher;

        public RabbitRepository(string boundedContext, string connectionString, IPublisher<DomainMessageCommit> eventStorePublisher, ProtoregSerializer serializer)
        {
            mssqlStore = new MssqlEventStore(boundedContext, connectionString, serializer);
            eventPublisher = eventStorePublisher;
        }

        public AR Update<AR>(IAggregateRootId aggregateId, Action<AR> update, Action<IAggregateRoot> save = null) where AR : IAggregateRoot
        {
            Action<IAggregateRoot> saveAction = save ?? this.Save;
            return mssqlStore.Update<AR>(aggregateId, update, saveAction);
        }

        public void Save(IAggregateRoot aggregateRoot)
        {
            if (aggregateRoot.UncommittedEvents == null || aggregateRoot.UncommittedEvents.Count == 0)
                return;
            aggregateRoot.State.Version += 1;
            var commit = new DomainMessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents);
            eventPublisher.Publish(commit);
        }
    }
}
