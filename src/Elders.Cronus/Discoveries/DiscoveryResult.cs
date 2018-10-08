using System;
using System.Collections.Generic;

namespace Elders.Cronus.Discoveries
{
    public interface IDiscoveryResult<out T>
    {

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
        public Type AbstractionType { get; set; }

        public Type ImplementationType { get; set; }

        public Func<object> Factory { get; set; }

        public object Instance { get; set; }
    }
}
