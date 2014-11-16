using Elders.Cronus.Pipeline.Hosts;
using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.IocContainer;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.EventSourcing.InMemory.Config;
using Elders.Cronus.EventSourcing.InMemory;
using Elders.Cronus.Tests.TestModel;

namespace Elders.Cronus.Tests.InMemoryEventStore
{
    public class When_using_in_memory_event_store
    {
        Establish context = () =>
        {
            container = new Container();
            var settings = new CronusSettings(container);
            var eventStoreSettings = new InMemoryEventStoreSettings(settings);
            eventStoreSettings.Build();
            eventStoreStorage = (InMemoryEventStoreStorage)container.Resolve(typeof(InMemoryEventStoreStorage));
        };

        Because of = () => eventStore = (IEventStore)container.Resolve(typeof(IEventStore));

        It should_instansiate_in_memory_event_store = () => eventStore.ShouldBeOfExactType<Elders.Cronus.EventSourcing.InMemory.InMemoryEventStore>();

        It should_have_in_memory_event_store_storage = () => eventStoreStorage.ShouldBeOfExactType<Elders.Cronus.EventSourcing.InMemory.InMemoryEventStoreStorage>();

        It should_have_in_memory_aggregate_repository = () => eventStore.AggregateRepository.ShouldBeOfExactType<Elders.Cronus.EventSourcing.InMemory.InMemoryAggregateRepository>();

        It should_have_in_memory_persister = () => eventStore.Persister.ShouldBeOfExactType<Elders.Cronus.EventSourcing.InMemory.InMemoryEventStorePersister>();

        It should_have_in_memory_player = () => eventStore.Player.ShouldBeOfExactType<Elders.Cronus.EventSourcing.InMemory.InMemoryEventStorePlayer>();

        It should_have_in_memory_storage_manager = () => eventStore.StorageManager.ShouldBeOfExactType<Elders.Cronus.EventSourcing.InMemory.InMemoryEventStoreStorageManager>();

        static Container container;
        static IEventStore eventStore;
        static InMemoryEventStoreStorage eventStoreStorage;
    }
}