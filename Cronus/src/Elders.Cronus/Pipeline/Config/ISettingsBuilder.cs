using System;

namespace Elders.Cronus.Pipeline.Config
{
    public interface ISettingsBuilder<T>
    {
        Lazy<T> Build();
    }
}