using Elders.Cronus.Pipeline.Hosts;
using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.IocContainer;
using Elders.Cronus.EventSourcing;

namespace Elders.Cronus.Tests.InMemoryEventStore
{
    public class When_using_in_memory_event_store
    {
        Establish context = () =>
        {
            container = new Container();
            var settings = new CronusSettings(container);

            settings.UseInMemoryEventStore();
        };

        Because of = () => eventStore = (IEventStore)container.Resolve(typeof(IEventStore));

        It should_instansiate_in_memory_event_store = () => eventStore.ShouldBeOfExactType<Elders.Cronus.EventSourcing.InMemory.InMemoryEventStore>();

        It should_have_in_memory_aggregate_repository = () => eventStore.AggregateRepository.ShouldBeOfExactType<Elders.Cronus.EventSourcing.InMemory.InMemoryAggregateRepository>();

        It should_have_in_memory_persister = () => eventStore.Persister.ShouldBeOfExactType<Elders.Cronus.EventSourcing.InMemory.InMemoryEventStorePersister>();

        It should_have_in_memory_player = () => eventStore.Player.ShouldBeOfExactType<Elders.Cronus.EventSourcing.InMemory.InMemoryEventStorePlayer>();

        It should_have_in_memory_storage_manager = () => eventStore.StorageManager.ShouldBeOfExactType<Elders.Cronus.EventSourcing.InMemory.InMemoryEventStoreStorageManager>();

        static Container container;
        static IEventStore eventStore;
    }
}