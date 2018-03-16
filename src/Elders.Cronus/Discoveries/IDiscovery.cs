using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Discoveries
{
    public interface IDiscovery
    {
        void Discover(ISettingsBuilder builder);
    }
}
