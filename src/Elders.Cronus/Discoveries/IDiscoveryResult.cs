using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public interface IDiscoveryResult<out T>
    {
        IEnumerable<DiscoveredModel> Models { get; }
    }

    public class DiscoveryResult<T> : IDiscoveryResult<T>
    {
        public DiscoveryResult()
        {
            Models = new List<DiscoveredModel>();
        }

        public DiscoveryResult(IEnumerable<DiscoveredModel> models)
        {
            Models = models;
        }

        public IEnumerable<DiscoveredModel> Models { get; protected set; }
    }

    public class DiscoveredModel : ServiceDescriptor
    {
        public DiscoveredModel(Type serviceType, object instance) : base(serviceType, instance) { }

        public DiscoveredModel(Type serviceType, Type implementationType, ServiceLifetime lifetime) : base(serviceType, implementationType, lifetime) { }

        public DiscoveredModel(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime) : base(serviceType, factory, lifetime) { }
    }
}
