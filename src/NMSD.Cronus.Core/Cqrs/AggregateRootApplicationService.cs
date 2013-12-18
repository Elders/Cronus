using System;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Core.Messaging;
using System.Runtime.Remoting.Messaging;
using Cronus.Core.EventStore;

namespace NMSD.Cronus.Core.Cqrs
{
    public interface IAggregateRootApplicationService : IMessageHandler
    {
        IEventStore EventStore { get; set; }

        IPublisher<MessageCommit> EventPublisher { get; set; }
    }
    public class AggregateRootApplicationService<AR> : IAggregateRootApplicationService where AR : IAggregateRoot
    {
        public IEventStore EventStore { get; set; }
        public IPublisher<MessageCommit> EventPublisher { get; set; }

        protected void UpdateAggregate(IAggregateRootId id, Action<AR> updateAr)
        {
            var state = EventStore.LoadAggregateState(id.Id);
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
            aggregateRoot.State.Version += 1;
            var commit = new MessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents);
            EventPublisher.Publish(commit);
        }
    }
}
