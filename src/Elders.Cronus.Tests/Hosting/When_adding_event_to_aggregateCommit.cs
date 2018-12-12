using Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo;
using Elders.Cronus.EventStore;
using Machine.Specifications;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Migrations.TestMigration;
using System;

namespace Elders.Cronus.Migrations
{
    [Subject("Migration")]
    public class When_cronus_startups_are_executed
    {
        Establish context = () =>
        {
            scanner = new CronusStartupScanner(new TestAssemblyScanner());
        };

        Because of = () => startupTypes = scanner.Scan().ToList();

        It should_find_all_bootstraps = () => startupTypes.Count.ShouldEqual(11);

        It should_order_the_startup_types = () =>
        {
            for (int i = 0; i < expectedOrderedType.Count; i++)
            {
                startupTypes[i].ShouldEqual(expectedOrderedType[i]);
            }
        };

        static CronusStartupScanner scanner;
        static List<Type> startupTypes;
        static List<Type> expectedOrderedType = new List<Type>()
        {
            typeof(TestAssemblyScanner.EnvironmentStartup),
            typeof(TestAssemblyScanner.ExternalResourceStartup),
            typeof(TestAssemblyScanner.ConfigurationStartup),
            typeof(TestAssemblyScanner.AggregatesStartup),
            typeof(TestAssemblyScanner.PortsStartup),
            typeof(TestAssemblyScanner.SagasStartup),
            typeof(TestAssemblyScanner.SecondProjectionsStartup),
            typeof(TestAssemblyScanner.ProjectionsStartup),
            typeof(TestAssemblyScanner.GatewaysStartup),
            typeof(TestAssemblyScanner.NoAttributeStartup),
            typeof(TestAssemblyScanner.RuntimeStartup)
        };
    }

    public class TestAssemblyScanner : IAssemblyScanner
    {
        public IEnumerable<Type> Scan()
        {
            yield return typeof(SagasStartup);
            yield return typeof(GatewaysStartup);
            yield return typeof(SecondProjectionsStartup);
            yield return typeof(AggregatesStartup);
            yield return typeof(ExternalResourceStartup);
            yield return typeof(ProjectionsStartup);
            yield return typeof(NoAttributeStartup);
            yield return typeof(ConfigurationStartup);
            yield return typeof(EnvironmentStartup);
            yield return typeof(RuntimeStartup);
            yield return typeof(PortsStartup);
        }

        [CronusStartup(Bootstraps.Environment)] public class EnvironmentStartup : ICronusStartup { public void Bootstrap() { } }
        [CronusStartup(Bootstraps.ExternalResource)] public class ExternalResourceStartup : ICronusStartup { public void Bootstrap() { } }
        [CronusStartup(Bootstraps.Configuration)] public class ConfigurationStartup : ICronusStartup { public void Bootstrap() { } }
        [CronusStartup(Bootstraps.Aggregates)] public class AggregatesStartup : ICronusStartup { public void Bootstrap() { } }
        [CronusStartup(Bootstraps.Ports)] public class PortsStartup : ICronusStartup { public void Bootstrap() { } }
        [CronusStartup(Bootstraps.Sagas)] public class SagasStartup : ICronusStartup { public void Bootstrap() { } }
        [CronusStartup(Bootstraps.Projections)] public class ProjectionsStartup : ICronusStartup { public void Bootstrap() { } }
        [CronusStartup(Bootstraps.Projections)] public class SecondProjectionsStartup : ICronusStartup { public void Bootstrap() { } }
        [CronusStartup(Bootstraps.Gateways)] public class GatewaysStartup : ICronusStartup { public void Bootstrap() { } }
        [CronusStartup(Bootstraps.Runtime)] public class RuntimeStartup : ICronusStartup { public void Bootstrap() { } }
        public class NoAttributeStartup : ICronusStartup { public void Bootstrap() { } }
    }
}
