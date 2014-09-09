using System;

namespace Elders.Cronus.Pipeline.Config
{
    public static class SettingsInstanceExtensions
    {
        public static T GetInstance<T>(this ISettingsBuilder<T> settings)
        {
            return settings.Build().Value;
        }

        public static Lazy<T> GetInstanceLazy<T>(this ISettingsBuilder<T> settings)
        {
            return settings.Build();
        }
    }
}