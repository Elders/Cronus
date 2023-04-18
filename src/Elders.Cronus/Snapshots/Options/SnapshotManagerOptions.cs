using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.Snapshots.Options
{
    public class SnapshotManagerOptions
    {
        public int SnapshotCreationTimeoutMilliseconds { get; set; } = 60000;
    }

    public class SnapshotManagerOptionsProvider : CronusOptionsProviderBase<SnapshotManagerOptions>
    {
        public const string SettingKey = "cronus:persistence";

        public SnapshotManagerOptionsProvider(IConfiguration configuration) : base(configuration)
        {
        }

        public override void Configure(SnapshotManagerOptions options)
        {
            configuration.GetSection(SettingKey).Bind(options);
        }
    }
}
