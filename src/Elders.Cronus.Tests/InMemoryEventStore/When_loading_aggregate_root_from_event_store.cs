using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.EventSourcing.InMemory;
using Elders.Cronus.Pipeline.Hosts;
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
    public class When_loading_aggregate_root_from_event_store
    {
        Establish context = () =>
        {
            versionService = new InMemoryAggregateVersionService();
            eventStoreStorage = new InMemoryEventStoreStorage();
            eventStorePersister = new InMemoryEventStorePersister(eventStoreStorage);
            eventStoreManager = new InMemoryEventStoreStorageManager();
            eventStorePlayer = new InMemoryEventStorePlayer(eventStoreStorage);
            aggregateRepository = new InMemoryAggregateRepository(eventStorePersister, eventStoreStorage, versionService);
            eventStoreManager.CreateStorage();
            id = new TestAggregateId();
            aggregateRoot = new TestAggregateRoot(id);
            aggregateRepository.Save<TestAggregateRoot>(aggregateRoot);
            aggregateRoot.Apply(new TestUpdateEvent(id, "When_build_aggregate_root_from_events"));
            aggregateRepository.Save<TestAggregateRoot>(aggregateRoot);
        };

        Because of = () => loadedAggregateRoot = aggregateRepository.Load<TestAggregateRoot>(id);

        It should_instansiate_aggregate_root = () => loadedAggregateRoot.ShouldNotBeNull();

        It should_instansiate_aggregate_root_with_valid_state = () => ((IAggregateRootStateManager)loadedAggregateRoot).State.Id.ShouldEqual(id);

        It should_instansiate_aggregate_root_with_latest_state = () => ((TestAggregateRootState)((IAggregateRootStateManager)loadedAggregateRoot).State).UpdatableField.ShouldEqual("When_build_aggregate_root_from_events");

        It should_instansiate_aggregate_root_with_latest_state_version = () => ((IAggregateRootStateManager)loadedAggregateRoot).State.Version.ShouldEqual(2);

        static TestAggregateId id;
        static InMemoryEventStoreStorage eventStoreStorage;
        static IAggregateVersionService versionService;
        static IEventStorePersister eventStorePersister;
        static IEventStoreStorageManager eventStoreManager;
        static IEventStorePlayer eventStorePlayer;
        static IAggregateRepository aggregateRepository;
        static TestAggregateRoot aggregateRoot;
        static TestAggregateRoot loadedAggregateRoot;
    }
}