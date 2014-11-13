using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.EventSourcing.InMemory;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.Tests.InMemoryEventStore
{
    [Subject("AggregateRoot")]
    public class When_saving_aggregate_root_in_event_store
    {
        Establish context = () =>
        {
            eventStorePersister = new InMemoryEventStorePersister();
            eventStoreManager = new InMemoryEventStoreStorageManager();
            eventStorePlayer = new InMemoryEventStorePlayer();
            aggregateRepository = new InMemoryAggregateRepository(eventStorePersister);
            eventStoreManager.CreateStorage();
            id = new TestAggregateId();
            aggregateRoot = new TestAggregateRoot(id);
        };

        Because of = () => aggregateRepository.Save<TestAggregateRoot>(aggregateRoot);

        It should_instansiate_aggregate_root = () => aggregateRepository.Load<TestAggregateRoot>(id).ShouldNotBeNull();

        It should_instansiate_aggregate_root_with_valid_state = () => ((IAggregateRootStateManager)aggregateRepository.Load<TestAggregateRoot>(id)).State.Id.ShouldEqual(id);

        static TestAggregateId id;
        static IEventStorePersister eventStorePersister;
        static IEventStoreStorageManager eventStoreManager;
        static IEventStorePlayer eventStorePlayer;
        static IAggregateRepository aggregateRepository;
        static TestAggregateRoot aggregateRoot;
    }
}