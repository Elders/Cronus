using Elders.Cronus.Discoveries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;

namespace Elders.Cronus.Tests.InMemoryEventStoreSuite;

public interface ITestService { }
public interface DefaultService : ITestService { }
public interface OverriderService : ITestService { }

public class OverriderDiscoveredModel : IDiscoveryResult<ITestService>
{
    public IEnumerable<DiscoveredModel> Models => new List<DiscoveredModel>()
    {
        new DiscoveredModel(typeof(ITestService), typeof(OverriderService), ServiceLifetime.Transient)
    };

    public Action<IServiceCollection> AddServices => throw new NotImplementedException();
}

public class CronusServicesProviderTest : CronusServicesProvider
{
    public CronusServicesProviderTest(IServiceCollection services, IConfiguration configuration) : base(services, configuration) { }

    protected virtual void Handle(DiscoveryResult<ITestService> discoveryResult) => AddServices(discoveryResult);
}
