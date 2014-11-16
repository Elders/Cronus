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
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Tests.InMemoryEventStore
{
    public class When_using_in_memory_aggregate_version_service
    {
        Establish context = () =>
        {
            container = new Container();
            var settings = new CronusSettings(container);
            var eventStoreSettings = new InMemoryEventStoreSettings(settings);
            eventStoreSettings.Build();
            versionService = (IAggregateVersionService)container.Resolve(typeof(IAggregateVersionService));
            id = new TestAggregateId();
            versionService.ReserveVersion(id, 1);
            versionService.ReserveVersion(id, 2);
        };

        Because of = () => version = versionService.ReserveVersion(id, 3);

        It should_instansiate_in_memory_aggregate_version_service = () => versionService.ShouldBeOfExactType<Elders.Cronus.EventSourcing.InMemory.InMemoryAggregateVersionService>();

        It should_be_correct_version = () => version.ShouldEqual(3);

        static TestAggregateId id;
        static Container container;
        static IAggregateVersionService versionService;
        static int version;
    }
}