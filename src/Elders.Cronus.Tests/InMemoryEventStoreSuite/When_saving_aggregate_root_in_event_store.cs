using Elders.Cronus.DomainModeling;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;
using Elders.Cronus.EventStore.InMemory;
using Elders.Cronus.EventStore;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.AtomicAction.InMemory;
using Elders.Cronus.IntegrityValidation;

namespace Elders.Cronus.Tests.InMemoryEventStoreSuite
{
    [Subject("AggregateRoot")]
    public class When_saving_aggregate_root_in_event_store
    {
        Establish context = () =>
        {
            versionService = new InMemoryAggregateRootAtomicAction();
            eventStoreStorage = new InMemoryEventStoreStorage();
            eventStore = new InMemoryEventStore(eventStoreStorage);
            eventStoreManager = new InMemoryEventStoreStorageManager();
            eventStorePlayer = new InMemoryEventStorePlayer(eventStoreStorage);
            integrityPpolicy = new EventStreamIntegrityPolicy();
            aggregateRepository = new AggregateRepository(eventStore, versionService, integrityPpolicy);
            eventStoreManager.CreateStorage();
            id = new TestAggregateId();
            aggregateRoot = new TestAggregateRoot(id);
        };

        Because of = () => aggregateRepository.Save<TestAggregateRoot>(aggregateRoot);

        It should_instansiate_aggregate_root = () => aggregateRepository.Load<TestAggregateRoot>(id).ShouldNotBeNull();

        It should_instansiate_aggregate_root_with_valid_state = () => aggregateRepository.Load<TestAggregateRoot>(id).State.Id.ShouldEqual(id);

        static TestAggregateId id;
        static InMemoryEventStoreStorage eventStoreStorage;
        static IAggregateRootAtomicAction versionService;
        static IEventStore eventStore;
        static IEventStoreStorageManager eventStoreManager;
        static IEventStorePlayer eventStorePlayer;
        static IAggregateRepository aggregateRepository;
        static TestAggregateRoot aggregateRoot;
        static IIntegrityPolicy<EventStream> integrityPpolicy;
    }
}
