using System;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus
{
    public sealed class BoundedContext
    {
        public const string SettingKey = "cronus_boundedcontext";

        private readonly IConfiguration configuration;

        public BoundedContext(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            Name = configuration.GetRequired(SettingKey);
        }

        public string Name { get; private set; }

        public override string ToString() => Name;
    }
}
