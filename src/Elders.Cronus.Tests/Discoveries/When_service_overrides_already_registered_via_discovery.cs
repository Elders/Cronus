using Machine.Specifications;
using Elders.Cronus.Discoveries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Tests.InMemoryEventStoreSuite
{
    [Subject("Discoveries")]
    public class When_service_overrides_already_registered_via_discovery
    {
        Establish context = () =>
        {
            services = new ServiceCollectionMock();
            IConfiguration configuration = new ConfigurationMock();
            provider = new CronusServicesProviderTest(services, configuration);

            var defaulServiceDiscoveryResult = new DiscoveryResult<ITestService>(
                new List<DiscoveredModel>()
                {
                    new DiscoveredModel(typeof(ITestService), typeof(DefaultService), ServiceLifetime.Transient)
                });

            provider.HandleDiscoveredModel(defaulServiceDiscoveryResult);

            var model = new DiscoveredModel(typeof(ITestService), typeof(OverriderService), ServiceLifetime.Transient);
            model.CanOverrideDefaults = true;
            overrideServiceDiscoveryResult = new DiscoveryResult<ITestService>(
                new List<DiscoveredModel>() { model });
        };

        Because of = () => provider.HandleDiscoveredModel(overrideServiceDiscoveryResult);

        It should_have_service_registered = () => services.Count.ShouldEqual(1);

        It should_have_correct_descriptor_service_type_registered = () => services.SingleOrDefault().ServiceType.ShouldEqual(typeof(ITestService));

        It should_have_correct_descriptor_implementation_type_registered = () => services.SingleOrDefault().ImplementationType.ShouldEqual(typeof(OverriderService));

        static ICronusServicesProvider provider;
        static IServiceCollection services;
        static IDiscoveryResult<ITestService> overrideServiceDiscoveryResult;
    }

    [Subject("Discoveries")]
    public class When_service_overrides_already_registered_via_discovery_new
    {
        Establish context = () =>
        {
            services = new ServiceCollectionMock();
            IConfiguration configuration = new ConfigurationMock();
            provider = new CronusServicesProviderTest(services, configuration);

            var defaulServiceDiscoveryResult = new DiscoveryResult<ITestService>(
                new List<DiscoveredModel>(),
                services => services.AddTransient<ITestService, DefaultService>());

            provider.HandleDiscoveredModel(defaulServiceDiscoveryResult);

            var model = new DiscoveredModel(typeof(ITestService), typeof(OverriderService), ServiceLifetime.Transient);
            model.CanOverrideDefaults = true;
            overrideServiceDiscoveryResult = new DiscoveryResult<ITestService>(
                new List<DiscoveredModel>() { model });
        };

        Because of = () => provider.HandleDiscoveredModel(overrideServiceDiscoveryResult);

        It should_have_service_registered = () => services.Count.ShouldEqual(1);

        It should_have_correct_descriptor_service_type_registered = () => services.SingleOrDefault().ServiceType.ShouldEqual(typeof(ITestService));

        It should_have_correct_descriptor_implementation_type_registered = () => services.SingleOrDefault().ImplementationType.ShouldEqual(typeof(OverriderService));

        static ICronusServicesProvider provider;
        static IServiceCollection services;
        static IDiscoveryResult<ITestService> overrideServiceDiscoveryResult;
    }
}
