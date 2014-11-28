using Elders.Cronus.Pipeline.Hosts;
using Machine.Specifications;
using Elders.Cronus.IocContainer;
using Elders.Cronus.EventStore.InMemory;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.InMemory.Config;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Tests.InMemoryEventStoreSuite
{
    public class When_using_in_memory_event_store
    {
        Establish context = () =>
        {
            container = new Container();
            container.RegisterSingleton<IPublisher<IEvent>>(() => new NulllEventPublisher());

            var settings = new CronusSettings(container);

            var eventStoreSettings = new InMemoryEventStoreSettings(settings);
            eventStoreSettings.Build();
            eventStoreStorage = (InMemoryEventStoreStorage)container.Resolve(typeof(InMemoryEventStoreStorage));
        };

        Because of = () => eventStore = (IEventStore)container.Resolve(typeof(IEventStore));

        It should_instansiate_in_memory_event_store = () => eventStore.ShouldBeOfExactType<InMemoryEventStore>();

        It should_have_in_memory_event_store_storage = () => eventStoreStorage.ShouldBeOfExactType<InMemoryEventStoreStorage>();

        It should_have_in_memory_persister = () => eventStore.Persister.ShouldBeOfExactType<InMemoryEventStorePersister>();

        It should_have_in_memory_player = () => eventStore.Player.ShouldBeOfExactType<InMemoryEventStorePlayer>();

        It should_have_in_memory_storage_manager = () => eventStore.StorageManager.ShouldBeOfExactType<InMemoryEventStoreStorageManager>();

        static Container container;
        static IEventStore eventStore;
        static InMemoryEventStoreStorage eventStoreStorage;
    }
}