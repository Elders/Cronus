using NMSD.Cronus.Pipelining.Config;
using NMSD.Cronus.Transports.RabbitMQ;

namespace NMSD.Cronus.Pipelining.RabbitMQ.Config
{
    public class RabbitMqTransportSettings : PipelineTransportSettings
    {
        public override void Build(PipelineSettings pipelineSettings)
        {
            var rabbitSessionFactory = new RabbitMqSessionFactory();
            var session = rabbitSessionFactory.OpenSession();

            PipelineFactory = new RabbitMqPipelineFactory(session, pipelineSettings.PipelineNameConvention);
            EndpointFactory = new RabbitMqEndpointFactory(session, PipelineFactory as RabbitMqPipelineFactory, pipelineSettings.EndpointNameConvention);
        }
    }
}