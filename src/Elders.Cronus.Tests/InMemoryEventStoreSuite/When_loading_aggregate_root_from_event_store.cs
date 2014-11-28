using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.InMemory;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;

namespace Elders.Cronus.Tests.InMemoryEventStoreSuite
{
    [Subject("AggregateRoot")]
    public class When_loading_aggregate_root_from_event_store
    {
        Establish context = () =>
        {

            versionService = new InMemoryAggregateRevisionService();
            eventStoreStorage = new InMemoryEventStoreStorage();
            eventStorePersister = new InMemoryEventStorePersister(eventStoreStorage);
            eventStoreManager = new InMemoryEventStoreStorageManager();
            eventStorePlayer = new InMemoryEventStorePlayer(eventStoreStorage);
            aggregateRepository = new AggregateRepository(eventStorePersister, new NulllEventPublisher(), versionService);
            eventStoreManager.CreateStorage();
            id = new TestAggregateId();
            aggregateRoot = new TestAggregateRoot(id);
            aggregateRepository.Save<TestAggregateRoot>(aggregateRoot);
            aggregateRoot = aggregateRepository.Load<TestAggregateRoot>(id);
            aggregateRoot.Update("When_build_aggregate_root_from_events");
            aggregateRepository.Save<TestAggregateRoot>(aggregateRoot);
        };

        Because of = () => loadedAggregateRoot = aggregateRepository.Load<TestAggregateRoot>(id);

        It should_instansiate_aggregate_root = () => loadedAggregateRoot.ShouldNotBeNull();

        It should_instansiate_aggregate_root_with_valid_state = () => ((IAggregateRootStateManager)loadedAggregateRoot).State.Id.ShouldEqual(id);

        It should_instansiate_aggregate_root_with_latest_state = () => ((TestAggregateRootState)((IAggregateRootStateManager)loadedAggregateRoot).State).UpdatableField.ShouldEqual("When_build_aggregate_root_from_events");

        It should_instansiate_aggregate_root_with_latest_state_version = () => (loadedAggregateRoot as IAggregateRoot).Revision.ShouldEqual(2);

        static TestAggregateId id;
        static InMemoryEventStoreStorage eventStoreStorage;
        static IAggregateRevisionService versionService;
        static IEventStorePersister eventStorePersister;
        static IEventStoreStorageManager eventStoreManager;
        static IEventStorePlayer eventStorePlayer;
        static IAggregateRepository aggregateRepository;
        static TestAggregateRoot aggregateRoot;
        static TestAggregateRoot loadedAggregateRoot;
    }
}