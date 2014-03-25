using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Pipeline.Transport.RabbitMQ.Config
{
    public class RabbitMq : PipelineTransportSettings
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