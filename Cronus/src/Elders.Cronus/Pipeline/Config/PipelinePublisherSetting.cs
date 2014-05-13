using System.Linq;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Pipeline.Transport.Config;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IPipelinePublisherBuilder<TContract>
        where TContract : IMessage
    {
        IPublisher<TContract> Build();
    }

    public abstract class PipelinePublisherSetting<TContract> : PublisherSettings, IHavePipelineTransport
        where TContract : IMessage
    {
        public PipelinePublisherSetting()
        {
            PipelineSettings = new PipelineSettings();
        }

        public IPipelineTransport Transport { get; set; }



        public IPipelineSettings PipelineSettings { get; set; }

        public IPipelineTransportSettings TransportSettings { get; set; }



        public IPublisher<TContract> Build()
        {
            if (Transport == null)
            {
                TransportSettings.PipelineSettings = PipelineSettings;
                Transport = TransportSettings.BuildPipelineTransport();
            }

            if (MessagesAssemblies != null)
                MessagesAssemblies.ToList().ForEach(ass => GlobalSettings.Protoreg.RegisterAssembly(ass));

            return BuildPublisher();
        }

        protected abstract IPublisher<TContract> BuildPublisher();

    }
}
