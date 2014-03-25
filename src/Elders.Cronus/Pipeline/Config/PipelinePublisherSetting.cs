using System.Linq;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class PipelinePublisherSetting<TContract> : PublisherSettings<TContract, PipelineTransportSettings> where TContract : IMessage
    {
        public PipelinePublisherSetting()
        {
            PipelineSettings = new PipelineSettings();
        }

        public PipelineSettings PipelineSettings { get; set; }

        public override IPublisher<TContract> Build()
        {
            Transport.Build(PipelineSettings);
            if (MessagesAssemblies != null)
                MessagesAssemblies.ToList().ForEach(ass => GlobalSettings.Protoreg.RegisterAssembly(ass));
            return BuildPublisher();
        }

    }
}
