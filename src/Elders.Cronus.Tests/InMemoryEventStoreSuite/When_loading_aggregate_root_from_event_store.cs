using Elders.Cronus.AtomicAction;
using Elders.Cronus.AtomicAction.InMemory;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.InMemory;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;

namespace Elders.Cronus.Tests.InMemoryEventStoreSuite
{
    [Subject("AggregateRoot")]
    public class When_loading_aggregate_root_from_event_store
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
            aggregateRepository.Save<TestAggregateRoot>(aggregateRoot);
            aggregateRoot = aggregateRepository.Load<TestAggregateRoot>(id);
            aggregateRoot.Update("When_build_aggregate_root_from_events");
            aggregateRepository.Save<TestAggregateRoot>(aggregateRoot);
        };

        Because of = () => loadedAggregateRoot = aggregateRepository.Load<TestAggregateRoot>(id);

        It should_instansiate_aggregate_root = () => loadedAggregateRoot.ShouldNotBeNull();

        It should_instansiate_aggregate_root_with_valid_state = () => loadedAggregateRoot.State.Id.ShouldEqual(id);

        It should_instansiate_aggregate_root_with_latest_state = () => loadedAggregateRoot.State.UpdatableField.ShouldEqual("When_build_aggregate_root_from_events");

        It should_instansiate_aggregate_root_with_latest_state_version = () => (loadedAggregateRoot as IAggregateRoot).Revision.ShouldEqual(2);

        static TestAggregateId id;
        static InMemoryEventStoreStorage eventStoreStorage;
        static IAggregateRootAtomicAction versionService;
        static IEventStore eventStore;
        static IEventStoreStorageManager eventStoreManager;
        static IEventStorePlayer eventStorePlayer;
        static IAggregateRepository aggregateRepository;
        static TestAggregateRoot aggregateRoot;
        static TestAggregateRoot loadedAggregateRoot;
        static IIntegrityPolicy<EventStream> integrityPpolicy;
    }
}
