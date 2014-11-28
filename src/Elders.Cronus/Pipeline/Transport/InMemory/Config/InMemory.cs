using System;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Pipeline.Transport.InMemory.Config
{
    public interface IInMemoryPipelineTransportSettings : ISettingsBuilder
    {

    }

    public class InMemoryPipelineTransportSettings : IInMemoryPipelineTransportSettings
    {
        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        void ISettingsBuilder.Build()
        {
            var builder = this as ISettingsBuilder;
            var pipelineNameConvention = builder.Container.Resolve<IPipelineNameConvention>(builder.Name);
            var endpointNameConvention = builder.Container.Resolve<IEndpointNameConvention>(builder.Name);
            var transport = new InMemoryPipelineTransport(pipelineNameConvention, endpointNameConvention);
            builder.Container.RegisterSingleton<IPipelineTransport>(() => transport, builder.Name);
        }
    }

    public static class InMemoryTransportExtensions
    {
        public static T UseInMemoryPipelineTransport<T>(this T self, Action<InMemoryPipelineTransportSettings> configure = null)
        {
            InMemoryPipelineTransportSettings settings = new InMemoryPipelineTransportSettings();
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }
    }
}