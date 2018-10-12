using System;
using System.Collections.Generic;

namespace Elders.Cronus.Discoveries
{
    public interface IDiscoveryResult<out T>
    {
        List<DiscoveredModel> Models { get; }
    }

    public class DiscoveryResult<T> : IDiscoveryResult<T>
    {
        public DiscoveryResult()
        {
            Models = new List<DiscoveredModel>();
        }

        public List<DiscoveredModel> Models { get; protected set; }
    }

    public class DiscoveredModel
    {
        public DiscoveredModel(Type implementationType)
        {
            if (implementationType is null) throw new ArgumentNullException(nameof(implementationType));

            AbstractionType = implementationType;
            ImplementationType = implementationType;
        }

        public DiscoveredModel(Type abstractionType, Type implementationType)
        {
            if (abstractionType is null) throw new ArgumentNullException(nameof(abstractionType));
            if (implementationType is null) throw new ArgumentNullException(nameof(implementationType));

            AbstractionType = abstractionType;
            ImplementationType = implementationType;
        }

        public DiscoveredModel(Type abstractionType, Type implementationType, object instance)
            : this(abstractionType, implementationType)
        {
            if (instance is null) throw new ArgumentNullException(nameof(instance));

            Instance = instance;
        }

        public DiscoveredModel(Type abstractionType, Type implementationType, Func<object> factory)
            : this(abstractionType, implementationType)
        {
            if (factory is null) throw new ArgumentNullException(nameof(factory));

            Factory = factory;
        }

        public Type AbstractionType { get; private set; }

        public Type ImplementationType { get; private set; }

        public Func<object> Factory { get; private set; }

        public object Instance { get; private set; }
    }
}
