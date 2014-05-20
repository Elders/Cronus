using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Messaging.MessageHandleScope;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.EventSourcing
{
    public interface IEventStore
    {
        IAggregateRepository AggregateRepository { get; }
        IEventStorePersister Persister { get; }
        IEventStorePlayer Player { get; }
        IEventStoreStorageManager StorageManager { get; }
    }

    public interface IEventStoreBatchContext { }

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

        public IEnumerable<Type> GetRegisteredHandlers()
        {
            yield return assemblyContainingEventsByEventType;
        }

        public ISafeBatchResult<DomainMessageCommit> Handle(List<DomainMessageCommit> commits)
        {
            var safeBatch = safeBatchFactory.Initialize();
            return safeBatch.Execute(commits);
        }

        public Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>> HandlerRegistrations { get; set; }

        public int BatchSize { get; set; }
    }
}