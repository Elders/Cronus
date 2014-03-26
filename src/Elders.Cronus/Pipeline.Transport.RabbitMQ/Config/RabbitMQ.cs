using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Pipeline.Transport.RabbitMQ.Config
{
    public class RabbitMq : PipelineTransportSettings
    {
        public static RabbitMqSession session;
        public override void Build(PipelineSettings pipelineSettings)
        {
            var rabbitSessionFactory = new RabbitMqSessionFactory();
            if (session == null)
                session = rabbitSessionFactory.OpenSession();

            PipelineFactory = new RabbitMqPipelineFactory(session, pipelineSettings.PipelineNameConvention);
            EndpointFactory = new RabbitMqEndpointFactory(session, PipelineFactory as RabbitMqPipelineFactory, pipelineSettings.EndpointNameConvention);
        }
    }
}