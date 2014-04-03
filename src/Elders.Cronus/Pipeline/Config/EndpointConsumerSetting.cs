using System.Linq;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Pipeline.Config
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
            ErrorStrategy = new ErrorEndpointPerEndpoint(Transport.PipelineFactory, GlobalSettings.Serializer);
            return BuildConsumer();
        }
    }
}
