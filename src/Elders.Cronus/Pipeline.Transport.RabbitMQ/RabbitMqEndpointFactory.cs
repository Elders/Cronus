using System;

namespace Elders.Cronus.Pipeline.Transport.RabbitMQ
{
    public class RabbitMqEndpointFactory : IEndpointFactory
    {
        private readonly IEndpointNameConvention endpointNameConvention;
        private readonly RabbitMqSession session;

        public RabbitMqEndpointFactory(RabbitMqSession session, IEndpointNameConvention endpointNameConvention)
        {
            this.endpointNameConvention = endpointNameConvention;
            this.session = session;
        }

        public IEndpoint CreateEndpoint(EndpointDefinition definition)
        {
            var endpoint = new RabbitMqEndpoint(definition, session);
            endpoint.RoutingHeaders.Add("x-match", "any");
            endpoint.Declare();

            var pipeLine = new RabbitMqPipeline(definition.PipelineName, session, RabbitMqPipeline.PipelineType.Headers);
            pipeLine.Declare();
            pipeLine.Bind(endpoint);
            return endpoint;
        }

        public IEndpoint CreateTopicEndpoint(EndpointDefinition definition)
        {
            var endpoint = new RabbitMqEndpoint(definition, session);
            endpoint.Declare();

            var pipeLine = new RabbitMqPipeline(definition.PipelineName, session, RabbitMqPipeline.PipelineType.Topics);
            pipeLine.Declare();
            pipeLine.Bind(endpoint);
            return endpoint;
        }

        public EndpointDefinition GetEndpointDefinition(params Type[] handlerTypes)
        {
            return endpointNameConvention.GetEndpointDefinition(handlerTypes);
        }
    }
}
