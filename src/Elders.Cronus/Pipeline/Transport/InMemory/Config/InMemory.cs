using System;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Pipeline.Transport.InMemory.Config
{
    public interface IInMemoryTransportSettings : ISettingsBuilder<IPipelineTransport>, IHavePipelineSettings
    {

    }

    public class InMemoryTransportSettings : IInMemoryTransportSettings
    {
        Lazy<IPipelineNameConvention> IHavePipelineSettings.PipelineNameConvention { get; set; }

        Lazy<IEndpointNameConvention> IHavePipelineSettings.EndpointNameConvention { get; set; }

        Lazy<IPipelineTransport> ISettingsBuilder<IPipelineTransport>.Build()
        {
            IInMemoryTransportSettings settings = this as IInMemoryTransportSettings;

            return new Lazy<IPipelineTransport>(() => new InMemoryPipelineTransport(settings.PipelineNameConvention.Value, settings.EndpointNameConvention.Value));
        }
    }

    public static class InMemoryTransportExtensions
    {
        public static T UseInMemoryTransport<T>(this T self, Action<InMemoryTransportSettings> configure = null)
                where T : IHaveTransport<IPipelineTransport>, IHavePipelineSettings
        {
            InMemoryTransportSettings transportSettingsInstance = new InMemoryTransportSettings();
            if (configure != null)
                configure(transportSettingsInstance);

            self.Transport = new Lazy<IPipelineTransport>(() =>
            {
                self.CopyPipelineSettingsTo(transportSettingsInstance);
                return transportSettingsInstance.GetInstanceLazy().Value;
            });

            return self;
        }
    }
}