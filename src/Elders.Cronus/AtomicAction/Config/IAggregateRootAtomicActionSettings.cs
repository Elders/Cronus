using System;
using Elders.Cronus.AtomicAction.InMemory;
//using Elders.Cronus.AtomicAction.InMemory;
using Elders.Cronus.Cluster.Config;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.AtomicAction.Config
{
    public interface IAggregateRootAtomicActionSettings : ISettingsBuilder
    {
        IAggregateRootAtomicAction AggregateRootAtomicAtion { get; set; }
    }

    public class AggregateRootAtomicActionSettings : SettingsBuilder, IAggregateRootAtomicActionSettings
    {
        public AggregateRootAtomicActionSettings(ISettingsBuilder settingsBuilder) : base(settingsBuilder)
        {
            //this.WithInMemory();
        }

        IAggregateRootAtomicAction IAggregateRootAtomicActionSettings.AggregateRootAtomicAtion { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var casted = this as IAggregateRootAtomicActionSettings;
            builder.Container.RegisterSingleton(() => casted.AggregateRootAtomicAtion, builder.Name);
        }
    }

    public static class AggregateAtomicActionSettingExtensions
    {
        public static T WithInMemory<T>(this T self) where T : IAggregateRootAtomicActionSettings
        {
            self.AggregateRootAtomicAtion = new InMemoryAggregateRootAtomicAction();
            return self;
        }

        public static T UseAggregateRootAtomicAction<T>(this T self, Action<AggregateRootAtomicActionSettings> configure = null)
            where T : IClusterSettings
        {
            var settings = new AggregateRootAtomicActionSettings(self);

            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();

            return self;
        }
    }
}
