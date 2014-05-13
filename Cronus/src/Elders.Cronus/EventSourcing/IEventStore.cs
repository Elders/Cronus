using System;
using System.Collections.Generic;

namespace Elders.Cronus.EventSourcing
{
    public interface IEventStoreBatchContext
    {

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