using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cronus.Core.EventStore;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Messaging;
using NMSD.Protoreg;

namespace NMSD.Cronus.Core.EventStoreEngine
{
    public class RabbitEventStore : IEventStore
    {
        private MssqlEventStore mssqlStore;

        private IPublisher<MessageCommit> eventPublisher;

        public RabbitEventStore(string boundedContext, string connectionString, ProtoregSerializer serializer)
        {
            mssqlStore = new MssqlEventStore(boundedContext, connectionString, serializer);
            eventPublisher = new RabbitEventStorePublisher(serializer);
        }

        public AR Load<AR>(Cqrs.IAggregateRootId aggregateId) where AR : Cqrs.IAggregateRoot
        {
            return mssqlStore.Load<AR>(aggregateId);
        }

        public void Save(Cqrs.IAggregateRoot aggregateRoot)
        {
            aggregateRoot.State.Version += 1;
            var commit = new MessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents);
            eventPublisher.Publish(commit);
        }
    }
}
