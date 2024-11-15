using System;
using System.Collections.Generic;

namespace Elders.Cronus.AutoUpdates;

public class AutoUpdaterStrategy : IAutoUpdaterStrategy
{
    Dictionary<string, IAutoUpdate> handlers = new Dictionary<string, IAutoUpdate>();

    public AutoUpdaterStrategy(IEnumerable<IAutoUpdate> autoUpdates)
    {
        Register(autoUpdates);
    }

    public IAutoUpdate GetInstanceFor(string name)
    {
        bool exists = handlers.TryGetValue(name, out IAutoUpdate concreteInstance);
        if (exists)
            return concreteInstance;
        else
            throw new InvalidOperationException($"Why you getting instance for something that isn't registered. Name: {name}");
    }

    public void Register(IEnumerable<IAutoUpdate> autoUpdates)
    {
        foreach (IAutoUpdate autoUpdate in autoUpdates)
        {
            handlers.Add(autoUpdate.Name, autoUpdate);
        }
    }
}
