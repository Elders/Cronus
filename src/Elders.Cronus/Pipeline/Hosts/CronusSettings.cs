using System;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Pipeline.Hosts
{
    public interface ICronusSettings : ICanConfigureSerializer, ISettingsBuilder { }

    public class CronusSettings : ICronusSettings
    {
        public CronusSettings(IContainer container)
        {
            // TODO: This is temporary instantiated here. We need to think where is the best place to set the default EventStreamIntegrityPolicy.
            container.RegisterSingleton<IntegrityValidation.IIntegrityPolicy<EventStore.EventStream>>(() => new EventStore.EventStreamIntegrityPolicy());

            (this as ISettingsBuilder).Container = container;
        }

        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        void ISettingsBuilder.Build()
        {
            var builder = this as ISettingsBuilder;
            var consumers = builder.Container.ResolveAll<ICronusConsumer>();
            CronusHost host = new CronusHost();
            host.Consumers = consumers;
            builder.Container.RegisterSingleton(typeof(CronusHost), () => host);
        }
    }
}
