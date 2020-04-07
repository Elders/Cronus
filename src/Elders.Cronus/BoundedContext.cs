using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus
{
    public sealed class BoundedContext
    {
        public const string SettingKey = "cronus:boundedcontext";
        public const string ValidBoundedContextRegex = @"^\b([\w\d_]+$)";

        private readonly IConfiguration configuration;

        public BoundedContext(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var boundedContext = configuration.GetRequired(SettingKey);
            EnsureBoundedContext(boundedContext);

            Name = boundedContext;
        }

        private void EnsureBoundedContext(string boundedContext)
        {
            var regex = new Regex(ValidBoundedContextRegex);

            if (regex.IsMatch(boundedContext) == false)
                throw new ArgumentException($"Invalid boundedContext `{boundedContext}`. For more information see here https://github.com/Elders/Cronus/blob/master/doc/Configuration.md", nameof(boundedContext));
        }

        public string Name { get; private set; }

        public override string ToString() => Name;
    }
}
