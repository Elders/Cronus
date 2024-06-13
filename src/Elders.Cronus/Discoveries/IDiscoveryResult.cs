using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries;

public interface IDiscoveryResult<out T>
{
    IEnumerable<DiscoveredModel> Models { get; }
    Action<IServiceCollection> AddServices { get; }
}

public class DiscoveryResult<T> : IDiscoveryResult<T>
{
    public DiscoveryResult() : this(Enumerable.Empty<DiscoveredModel>()) { }

    public DiscoveryResult(IEnumerable<DiscoveredModel> models) : this(models, s => { }) { }

    public DiscoveryResult(IEnumerable<DiscoveredModel> models, Action<IServiceCollection> servicesAction)
    {
        Models = models;
        AddServices = servicesAction;
    }

    public IEnumerable<DiscoveredModel> Models { get; protected set; }

    public Action<IServiceCollection> AddServices { get; protected set; }
}

public class DiscoveredModel : ServiceDescriptor
{
    public DiscoveredModel(Type serviceType, object instance) : base(serviceType: serviceType, provider => instance, lifetime: ServiceLifetime.Singleton) { } // This is singleton

    public DiscoveredModel(Type serviceType, Type implementationType, ServiceLifetime lifetime) : base(serviceType, implementationType, lifetime) { }

    public DiscoveredModel(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime) : base(serviceType, factory, lifetime) { }

    public bool CanOverrideDefaults { get; set; }

    public bool CanAddMultiple { get; set; }
}
