using Elders.Cronus.AtomicAction;
using Elders.Cronus.AtomicAction.InMemory;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.InMemory;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;

namespace Elders.Cronus.Tests.InMemoryEventStoreSuite
{
    [Subject("Entity")]
    public class When_loading_aggregate_root_with_entity_from_event_store
    {
        Establish context = async () =>
        {
            versionService = new InMemoryAggregateRootAtomicAction();
            eventStoreStorage = new InMemoryEventStoreStorage();
            eventStore = new InMemoryEventStore(eventStoreStorage);
            eventStoreManager = new InMemoryEventStoreStorageManager();
            eventStorePlayer = new InMemoryEventStorePlayer(eventStoreStorage);
            integrityPpolicy = new EventStreamIntegrityPolicy();
            eventStoreFactory = new EventStoreFactory(eventStore, new NoAggregateCommitTransformer(), null);
            aggregateRepository = new AggregateRepository(eventStoreFactory, versionService, integrityPpolicy);
            eventStoreManager.CreateStorage();

            id = new TestAggregateId();
            aggregateRoot = new TestAggregateRoot(id);
            await aggregateRepository.SaveAsync<TestAggregateRoot>(aggregateRoot);

            var events = await aggregateRepository.LoadAsync<TestAggregateRoot>(id).ConfigureAwait(false);
            aggregateRoot = events.Data;
            var entityId0 = new TestEntityId(id);
            aggregateRoot.CreateEntity(entityId0);
            await aggregateRepository.SaveAsync<TestAggregateRoot>(aggregateRoot);

            events = await aggregateRepository.LoadAsync<TestAggregateRoot>(id).ConfigureAwait(false);
            aggregateRoot = events.Data;
            var entityId1 = new TestEntityId(id);
            aggregateRoot.CreateEntity(entityId1);
            await aggregateRepository.SaveAsync<TestAggregateRoot>(aggregateRoot);
        };

        Because of = () => loadedAggregateRoot = aggregateRepository.LoadAsync<TestAggregateRoot>(id).GetAwaiter().GetResult().Data;

        It should_instansiate_aggregate_root = () => loadedAggregateRoot.ShouldNotBeNull();

        It should_instansiate_aggregate_root_with_valid_state = () => loadedAggregateRoot.State.Id.ShouldEqual(id);

        It should_instansiate_aggregate_root_with_latest_state = () => loadedAggregateRoot.State.Entities.Count.ShouldEqual(2);

        It should_instansiate_aggregate_root_with_latest_state_version = () => (loadedAggregateRoot as IAggregateRoot).Revision.ShouldEqual(3);

        static TestAggregateId id;
        static InMemoryEventStoreStorage eventStoreStorage;
        static IAggregateRootAtomicAction versionService;
        static IEventStore eventStore;
        static InMemoryEventStoreStorageManager eventStoreManager;
        static IEventStorePlayer eventStorePlayer;
        static IAggregateRepository aggregateRepository;
        static TestAggregateRoot aggregateRoot;
        static TestAggregateRoot loadedAggregateRoot;
        static IIntegrityPolicy<EventStream> integrityPpolicy;
        static EventStoreFactory eventStoreFactory;
    }
}
