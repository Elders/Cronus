using System;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.EventSourcing
{
    public class RabbitRepository : IAggregateRepository
    {
        private readonly IAggregateRepository aggregateRepository;

        private IPublisher eventPublisher;

        public RabbitRepository(IAggregateRepository aggregateRepository, IPublisher eventStorePublisher)
        {
            this.aggregateRepository = aggregateRepository;
            eventPublisher = eventStorePublisher;
        }

        public AR Update<AR>(IAggregateRootId aggregateId, ICommand command, Action<AR> update, Action<IAggregateRoot, ICommand> save = null) where AR : IAggregateRoot
        {
            Action<IAggregateRoot, ICommand> saveAction = save ?? this.Save;
            return aggregateRepository.Update<AR>(aggregateId, command, update, saveAction);
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
