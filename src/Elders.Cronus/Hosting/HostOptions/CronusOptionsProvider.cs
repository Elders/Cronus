using Microsoft.Extensions.Configuration;
using System;

namespace Elders.Cronus.Hosting.HostOptions
{
    public class CronusOptionsProvider : CronusOptionsProviderBase<CronusOptions>
    {
        public CronusOptionsProvider(IConfiguration configuration) : base(configuration) { }
        public override void Configure(CronusOptions options)
        {
            if (configuration.GetSection("Cronus").Exists() == false) throw new ArgumentNullException();

            options.BoundedContext = configuration.GetSection("Cronus").GetSection("Transport").GetRequired(nameof(options.BoundedContext));
        }
    }

    public class CronusOptions
    {
        public string BoundedContext { get; set; }
    }
}
