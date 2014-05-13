using System;
using System.Linq;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Pipeline.Transport.Config;
using Elders.Cronus.Pipeline;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class EndpointConsumerSetting<TContract> : ConsumerSettings, IEndpointConsumerSetting<TContract>
        where TContract : IMessage
    {
        public EndpointConsumerSetting()
        {
            PipelineSettings = new PipelineSettings();
            TransportSettings = new PipelineTransportSettings();
        }

        public IPipelineTransport Transport { get; set; }


        public IPipelineSettings PipelineSettings { get; set; }

        public IPipelineTransportSettings TransportSettings { get; set; }

        private void BuildTransport()
        {
            if (TransportInstance == null)
            {
                TransportSettings.PipelineSettings = PipelineSettings;
                Transport = TransportSettings.BuildPipelineTransport();
            }
            else
            {
                Transport = TransportInstance();
            }
        }

        public IEndpointConsumable<TContract> Build()
        {
            BuildTransport();
            PostConsume = ((IEndpointPostConsumeBuilder)this).BuildPostConsumeActions();

            if (MessagesAssemblies != null)
                MessagesAssemblies.ToList().ForEach(ass => GlobalSettings.Protoreg.RegisterAssembly(ass));

            return BuildConsumer();
        }

        public IEndpointPostConsume PostConsume { get; set; }
        public Func<IEndpointPostConsume> PostConsumeInstance { get; set; }
        public Func<IEndpointConsumerSuccessStrategy> SuccessStrategy { get; set; }
        public Func<IEndpointConsumerErrorStrategy> ErrorStrategy { get; set; }



        public Func<IPipelineTransport> TransportInstance { get; set; }

        IEndpointPostConsume IEndpointPostConsumeBuilder.BuildPostConsumeActions()
        {
            var postConsume = PostConsumeInstance != null
                ? PostConsumeInstance()
                : new NoEndpointPostConsume();

            if (SuccessStrategy != null)
                postConsume.SuccessStrategy = SuccessStrategy();
            if (ErrorStrategy != null)
                postConsume.ErrorStrategy = ErrorStrategy();

            return postConsume;
        }

        protected abstract IEndpointConsumable<TContract> BuildConsumer();

        IPipelineTransport IPipelineTransportBuilder.BuildPipelineTransport() { throw new NotImplementedException("Unknown transport"); }
    }
}
