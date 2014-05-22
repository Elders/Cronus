using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.EventSourcing
{
    public class RabbitRepository : IAggregateRepository
    {
        private readonly IAggregateRepository aggregateRepository;

        private IPublisher<DomainMessageCommit> eventPublisher;

        public RabbitRepository(IAggregateRepository aggregateRepository, IPublisher<DomainMessageCommit> eventStorePublisher)
        {
            this.aggregateRepository = aggregateRepository;
            eventPublisher = eventStorePublisher;
        }

        public AR Update<AR>(IAggregateRootId aggregateId, ICommand command, Action<AR> update, Action<IAggregateRoot, ICommand> save = null) where AR : IAggregateRoot
        {
            Action<IAggregateRoot, ICommand> saveAction = save ?? this.Save;
            return aggregateRepository.Update<AR>(command, update, saveAction);
        }

        public void Save(IAggregateRoot aggregateRoot, ICommand command)
        {
            if (aggregateRoot.UncommittedEvents == null || aggregateRoot.UncommittedEvents.Count == 0)
                return;
            aggregateRoot.State.Version += 1;
            var commit = new DomainMessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents, command);
            eventPublisher.Publish(commit);
        }


        public AR Update<AR>(ICommand command, Action<AR> update, Action<IAggregateRoot, ICommand> save = null) where AR : IAggregateRoot
        {
            Action<IAggregateRoot, ICommand> saveAction = save ?? this.Save;
            return aggregateRepository.Update<AR>(command, update, saveAction);
        }
    }

    public class InternalBatchRepository : IAggregateRepository
    {
        public List<DomainMessageCommit> Commits { get; set; }

        private readonly IAggregateRepository aggregateRepository;

        private IPublisher<IEvent> eventPublisher;

        public InternalBatchRepository(IAggregateRepository aggregateRepository)
        {
            this.aggregateRepository = aggregateRepository;
            Commits = new List<DomainMessageCommit>();
        }

        public AR Update<AR>(IAggregateRootId aggregateId, ICommand command, Action<AR> update, Action<IAggregateRoot, ICommand> save = null) where AR : IAggregateRoot
        {
            Action<IAggregateRoot, ICommand> saveAction = save ?? this.Save;
            return aggregateRepository.Update<AR>(command, update, saveAction);
        }

        public void Save(IAggregateRoot aggregateRoot, ICommand command)
        {
            if (aggregateRoot.UncommittedEvents == null || aggregateRoot.UncommittedEvents.Count == 0)
                return;
            aggregateRoot.State.Version += 1;
            var commit = new DomainMessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents, command);
            Commits.Add(commit);
        }


        public AR Update<AR>(ICommand command, Action<AR> update, Action<IAggregateRoot, ICommand> save = null) where AR : IAggregateRoot
        {
            Action<IAggregateRoot, ICommand> saveAction = save ?? this.Save;
            return aggregateRepository.Update<AR>(command, update, saveAction);
        }
    }
}
