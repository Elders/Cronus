using System.Linq;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Pipeline.Config
{
    public abstract class EndpointConsumerSetting<TContract> : ConsumerSettings<TContract, PipelineTransportSettings>, IEndpointConsumerSetting where TContract : IMessage
    {
        public EndpointConsumerSetting()
        {
            PipelineSettings = new PipelineSettings();
        }

        public PipelineSettings PipelineSettings { get; set; }

        public override IEndpointConsumable Build()
        {
            Transport.Build(PipelineSettings);
            if (MessagesAssemblies != null)
                MessagesAssemblies.ToList().ForEach(ass => GlobalSettings.Protoreg.RegisterAssembly(ass));
            return BuildConsumer();
        }
    }
}
