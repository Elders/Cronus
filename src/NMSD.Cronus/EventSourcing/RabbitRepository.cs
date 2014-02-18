using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Transports.Conventions;
using NMSD.Cronus.Transports.RabbitMQ;
using NMSD.Protoreg;

namespace NMSD.Cronus.EventSourcing
{
    public class RabbitRepository : IAggregateRepository
    {
        private MssqlEventStore mssqlStore;

        private IPublisher<DomainMessageCommit> eventPublisher;

        public RabbitRepository(string boundedContext, string connectionString, RabbitMqSession session, ProtoregSerializer serializer)
        {
            mssqlStore = new MssqlEventStore(boundedContext, connectionString, serializer);
            eventPublisher = new EventStorePublisher(new EventStorePipelinePerApplication(), new RabbitMqPipelineFactory(session), serializer);
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
