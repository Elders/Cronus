using Elders.Cronus.DomainModeling;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.EventStore.InMemory;
using Elders.Cronus.EventStore;

namespace Elders.Cronus.Tests.InMemoryEventStoreSuite
{
    [Subject("AggregateRoot")]
    public class When_loading_events_for_replay
    {
        Establish context = () =>
        {
            versionService = new InMemoryAggregateRevisionService();
            eventStoreStorage = new InMemoryEventStoreStorage();
            eventStore = new InMemoryEventStore(eventStoreStorage);
            eventStoreManager = new InMemoryEventStoreStorageManager();
            eventStorePlayer = new InMemoryEventStorePlayer(eventStoreStorage);
            aggregateRepository = new AggregateRepository(eventStore, new NulllEventPublisher(), versionService);
            eventStoreManager.CreateStorage();

            id = new TestAggregateId();

            //  1
            aggregateRoot = new TestAggregateRoot(id);
            aggregateRepository.Save<TestAggregateRoot>(aggregateRoot);

            //  2
            aggregateRoot = aggregateRepository.Load<TestAggregateRoot>(id);
            aggregateRoot.Update("When_build_aggregate_root_from_events");
            aggregateRepository.Save<TestAggregateRoot>(aggregateRoot);

            //  3
            aggregateRoot = aggregateRepository.Load<TestAggregateRoot>(id);
            aggregateRoot.Update("When_build_aggregate_root_from_events");
            aggregateRepository.Save<TestAggregateRoot>(aggregateRoot);
        };

        Because of = () => events = eventStorePlayer.GetEventsFromStart().ToList();

        It should_load_all_events = () => events.Count.ShouldEqual(3);

        static TestAggregateId id;
        static InMemoryEventStoreStorage eventStoreStorage;
        static IAggregateRevisionService versionService;
        static IEventStore eventStore;
        static IEventStoreStorageManager eventStoreManager;
        static IEventStorePlayer eventStorePlayer;
        static IAggregateRepository aggregateRepository;
        static TestAggregateRoot aggregateRoot;
        static TestAggregateRoot secondAggregateRoot;
        static List<IEvent> events;
    }
}