using System;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus
{
    public static class ConfigurationExtensions
    {
        public static string GetRequired(this IConfiguration configuration, string key)
        {
            string value = configuration[key];
            if (string.IsNullOrEmpty(value)) throw new ArgumentException($"Missing setting: {key}");

            return value;
        }
    }
}
