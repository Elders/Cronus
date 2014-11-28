using Elders.Cronus.Pipeline.Hosts;
using Machine.Specifications;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Tests.TestModel;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventStore.InMemory.Config;

namespace Elders.Cronus.Tests.InMemoryEventStoreSuite
{
    public class When_using_in_memory_aggregate_version_service
    {
        Establish context = () =>
        {
            container = new Container();
            var settings = new CronusSettings(container);
            var eventStoreSettings = new InMemoryEventStoreSettings(settings);
            eventStoreSettings.Build();
            versionService = (IAggregateRevisionService)container.Resolve(typeof(IAggregateRevisionService));
            id = new TestAggregateId();
            versionService.ReserveRevision(id, 1);
            versionService.ReserveRevision(id, 2);
        };

        Because of = () => version = versionService.ReserveRevision(id, 3);

        It should_instansiate_in_memory_aggregate_version_service = () => versionService.ShouldBeOfExactType<InMemoryAggregateRevisionService>();

        It should_be_correct_version = () => version.ShouldEqual(3);

        static TestAggregateId id;
        static Container container;
        static IAggregateRevisionService versionService;
        static int version;
    }
}