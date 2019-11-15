using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus
{
    public static class ConfigurationExtensions
    {
        public static string GetRequired(this IConfiguration configuration, string key)
        {
            string value = configuration[key];
            if (string.IsNullOrEmpty(value)) throw new ArgumentException($"Missing setting: {key} (for reference see here  https://github.com/Elders/Cronus/blob/master/doc/Configuration.md)");

            return value;
        }

        public static string GetRequired(this IConfigurationSection configuration, string key)
        {
            string value = configuration.GetChildren().FirstOrDefault(item => item.Key == key)?.Value;
            if (string.IsNullOrEmpty(value)) throw new ArgumentException($"Missing setting: {configuration.Key}:{key} (for reference see here  https://github.com/Elders/Cronus/blob/master/doc/Configuration.md)");

            return value;
        }

        public static string GetOptional(this IConfiguration configuration, string key, string defaultValue)
        {
            string value = configuration[key];

            if (string.IsNullOrEmpty(value) == false) return value;
            if (string.IsNullOrEmpty(defaultValue)) throw new ArgumentException($"Missing setting: {key} (for reference see here  https://github.com/Elders/Cronus/blob/master/doc/Configuration.md)");

            return defaultValue;
        }
    }
}
