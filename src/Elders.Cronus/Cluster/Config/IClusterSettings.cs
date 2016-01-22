using System;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.Cluster.Config
{
    public interface IClusterSettings : ISettingsBuilder
    {
        string ClusterName { get; set; }
    }

    public class ClusterSettings : SettingsBuilder, IClusterSettings
    {
        public ClusterSettings(ISettingsBuilder settingsBuilder) : base(settingsBuilder)
        {
        }

        string IClusterSettings.ClusterName { get; set; }

        public override void Build()
        {
        }
    }

    public static class ClusterSettingExtensions
    {
        public static T UseCluster<T>(this T self, Action<ClusterSettings> configure = null) where T : ICronusSettings
        {
            var settings = new ClusterSettings(self);

            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();

            return self;
        }
    }
}
