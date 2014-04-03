using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Messaging.MessageHandleScope;

namespace Elders.Cronus.EventSourcing
{
    public interface IEventStoreBatchContext
    {

    }

    public interface IEventStorePlayer
    {
        IEnumerable<IEvent> GetEventsFromStart(int batchPerQuery = 1);
    }

    public interface IEventStorePersister
    {
        void Persist(List<DomainMessageCommit> commits);
    }

    public class EventStoreHandler : IMessageProcessor<DomainMessageCommit>
    {
        private readonly Type assemblyContainingEventsByEventType;
        private readonly SafeBatchFactory<DomainMessageCommit, IEventStoreBatchContext> safeBatchFactory;


        public EventStoreHandler(Type assemblyContainingEventsByEventType, SafeBatchFactory<DomainMessageCommit, IEventStoreBatchContext> safeBatchFactory, int batchSize = 1)
        {
            this.safeBatchFactory = safeBatchFactory;
            this.assemblyContainingEventsByEventType = assemblyContainingEventsByEventType;
            BatchSize = batchSize;
        }

        public ScopeFactory ScopeFactory { get; set; }

        public IEnumerable<Type> GetRegisteredHandlers()
        {
            yield return assemblyContainingEventsByEventType;
        }

        public SafeBatchResult<DomainMessageCommit> Handle(List<DomainMessageCommit> commits)
        {
            var safeBatch = safeBatchFactory.Initialize();
            return safeBatch.Execute(commits);
        }

        public int BatchSize { get; private set; }
    }
}