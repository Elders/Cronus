using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Elders.Cronus.Hosting.Heartbeat
{
    public class HeartbeatOptions
    {
        [Range(5, 3600, ErrorMessage = "The configuration `Cronus:Heartbeat:IntervalInSeconds` cannot be negative as it represents a time interval in seconds.")]
        public uint IntervalInSeconds { get; set; } = 5;
    }

    public class HeartbeaOptionsProvider : CronusOptionsProviderBase<HeartbeatOptions>
    {
        public HeartbeaOptionsProvider(IConfiguration configuration) : base(configuration) { }

        public override void Configure(HeartbeatOptions options)
        {
            configuration.GetSection("Cronus:Heartbeat").Bind(options);
        }
    }
}
