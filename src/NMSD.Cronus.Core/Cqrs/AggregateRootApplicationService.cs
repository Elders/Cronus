using System;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Core.Messaging;
using System.Runtime.Remoting.Messaging;

namespace NMSD.Cronus.Core.Cqrs
{
    public interface IAggregateRootApplicationService : IMessageHandler
    {
        ProtoEventStore EventStore { get; set; }

        IPublisher<MessageCommit> EventPublisher { get; set; }
    }
    public class AggregateRootApplicationService<AR> : IAggregateRootApplicationService where AR : IAggregateRoot
    {
        public ProtoEventStore EventStore { get; set; }
        public IPublisher<MessageCommit> EventPublisher { get; set; }

        protected void UpdateAggregate(IAggregateRootId id, Action<AR> updateAr)
        {
            var state = EventStore.LoadAggregateState("Collaboration", id.Id);
            AR aggregateRoot = AggregateRootFactory.Build<AR>(state);
            updateAr(aggregateRoot);

            PublishCommit(aggregateRoot);
        }

        protected void CreateAggregate(AR aggregateRoot)
        {
            PublishCommit(aggregateRoot);
        }

        private void PublishCommit(IAggregateRoot aggregateRoot)
        {
            var commit = new MessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents);
            EventPublisher.Publish(commit);
        }
    }
}
