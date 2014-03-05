using NMSD.Cronus.Messaging;
using NMSD.Cronus.Pipelining.Transport.Config;
using NMSD.Cronus.Transport.InMemory;
using NMSD.Cronus.Transports.RabbitMQ;

namespace NMSD.Cronus.Pipelining.RabbitMQ.Config
{
    public class RabbitMqTransportSettings<T> : PipelineTransportSettings<T> where T : ITransportIMessage
    {
        public RabbitMqTransportSettings(PipelineSettings pipelineSettings = null)
            : base(pipelineSettings)
        { }

        public override void Build()
        {
            base.Build();

            RabbitSessionFactory = new RabbitMqSessionFactory();
            var session = RabbitSessionFactory.OpenSession();
            PipelineFactory = new RabbitMqPipelineFactory(session, PipelineSettings.PipelineNameConvention);
            EndpointFactory = new RabbitMqEndpointFactory(session, PipelineFactory as RabbitMqPipelineFactory, PipelineSettings.EndpointNameConvention);
        }

        public RabbitMqSessionFactory RabbitSessionFactory { get; set; }
    }
}