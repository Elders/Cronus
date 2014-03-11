using System;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.EventSourcing
{
    public class RabbitRepository : IAggregateRepository
    {
        private readonly IAggregateRepository eventStore;

        private IPublisher eventPublisher;

        public RabbitRepository(IAggregateRepository eventStore, IPublisher eventStorePublisher)
        {
            this.eventStore = eventStore;
            eventPublisher = eventStorePublisher;
        }

        public AR Update<AR>(IAggregateRootId aggregateId, ICommand command, Action<AR> update, Action<IAggregateRoot, ICommand> save = null) where AR : IAggregateRoot
        {
            Action<IAggregateRoot, ICommand> saveAction = save ?? this.Save;
            return eventStore.Update<AR>(aggregateId, command, update, saveAction);
        }

        public void Save(IAggregateRoot aggregateRoot, ICommand command)
        {
            if (aggregateRoot.UncommittedEvents == null || aggregateRoot.UncommittedEvents.Count == 0)
                return;
            aggregateRoot.State.Version += 1;
            var commit = new DomainMessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents, command);
            eventPublisher.Publish(commit);
        }
    }
}
