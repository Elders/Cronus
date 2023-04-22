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
            eventStorePlayer = new InMemoryEventStorePlayer(eventStoreStorage);
            integrityPpolicy = new EventStreamIntegrityPolicy();
            eventStoreFactory = new EventStoreFactory(eventStore, null);
            aggregateRepository = new AggregateRepository(eventStoreFactory, versionService, integrityPpolicy, new EmptyAggregateTransformer());
            id = new TestAggregateId();
            aggregateRoot = new TestAggregateRoot(id);
        };

        Because of = async () => await aggregateRepository.SaveAsync(aggregateRoot).ConfigureAwait(false);

        It should_instansiate_aggregate_root = async () => (await aggregateRepository.LoadAsync<TestAggregateRoot>(id).ConfigureAwait(false)).ShouldNotBeNull();

        It should_instansiate_aggregate_root_with_valid_state = async () => (await aggregateRepository.LoadAsync<TestAggregateRoot>(id).ConfigureAwait(false)).Data.State.Id.ShouldEqual(id);

        static TestAggregateId id;
        static InMemoryEventStoreStorage eventStoreStorage;
        static IAggregateRootAtomicAction versionService;
        static IEventStore eventStore;
        static IEventStorePlayer eventStorePlayer;
        static IAggregateRepository aggregateRepository;
        static TestAggregateRoot aggregateRoot;
        static IIntegrityPolicy<EventStream> integrityPpolicy;
        static EventStoreFactory eventStoreFactory;
    }
}
