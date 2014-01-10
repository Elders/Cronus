using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Core.DomainModelling;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Transports.Conventions;
using NMSD.Cronus.Core.Transports.RabbitMQ;
using NMSD.Protoreg;

namespace NMSD.Cronus.Core.EventSourcing
{
    public class RabbitEventStore : IAggregateRepository
    {
        private MssqlEventStore mssqlStore;

        private IPublisher<DomainMessageCommit> eventPublisher;

        public RabbitEventStore(string boundedContext, string connectionString, RabbitMqSession session, ProtoregSerializer serializer)
        {
            mssqlStore = new MssqlEventStore(boundedContext, connectionString, serializer);
            eventPublisher = new EventStorePublisher(new EventStorePipelinePerApplication(), new RabbitMqPipelineFactory(session), serializer);
        }

        public AR Load<AR>(IAggregateRootId aggregateId) where AR : IAggregateRoot
        {
            return mssqlStore.Load<AR>(aggregateId);
        }

        public void Save(IAggregateRoot aggregateRoot)
        {
            aggregateRoot.State.Version += 1;
            var commit = new DomainMessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents);
            eventPublisher.Publish(commit);
        }
    }
}
