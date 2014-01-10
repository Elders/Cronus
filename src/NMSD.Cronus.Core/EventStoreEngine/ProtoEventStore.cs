using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cronus.Core.EventStore;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Transports.Conventions;
using NMSD.Cronus.Core.Transports.RabbitMQ;
using NMSD.Protoreg;

namespace NMSD.Cronus.Core.EventStoreEngine
{
    public class RabbitEventStore : IEventStore
    {
        private MssqlEventStore mssqlStore;

        private IPublisher<DomainMessageCommit> eventPublisher;

        public RabbitEventStore(string boundedContext, string connectionString, RabbitMqSession session, ProtoregSerializer serializer)
        {
            mssqlStore = new MssqlEventStore(boundedContext, connectionString, serializer);
            eventPublisher = new EventStorePublisher(new EventStorePipelinePerApplication(), new RabbitMqPipelineFactory(session), serializer);
        }

        public AR Load<AR>(Cqrs.IAggregateRootId aggregateId) where AR : Cqrs.IAggregateRoot
        {
            return mssqlStore.Load<AR>(aggregateId);
        }

        public void Save(Cqrs.IAggregateRoot aggregateRoot)
        {
            aggregateRoot.State.Version += 1;
            var commit = new DomainMessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents);
            eventPublisher.Publish(commit);
        }
    }
}
