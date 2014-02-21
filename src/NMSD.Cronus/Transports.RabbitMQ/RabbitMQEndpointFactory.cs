using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.RabbitMQ;
using NMSD.Cronus.Transports.Conventions;

namespace NMSD.Cronus.Transports.RabbitMQ
{
    public class RabbitMqEndpointFactory : IEndpointFactory
    {
        private readonly IEndpointNameConvention endpointNameConvention;
        private RabbitMqPipelineFactory pipeFactory;
        private RabbitMqSession session;

        public RabbitMqEndpointFactory(RabbitMqSession session, IEndpointNameConvention endpointNameConvention, RabbitMqPipelineFactory pipelineFactory)
        {
            this.endpointNameConvention = endpointNameConvention;
            this.session = session;
            pipeFactory = pipelineFactory;
        }

        public IEndpoint CreateEndpoint(EndpointDefinition definition)
        {
            var endpoint = new RabbitMqEndpoint(definition.EndpointName, session);
            foreach (var header in definition.AcceptanceHeaders)
            {
                endpoint.RoutingHeaders.Add(header.Key, header.Value);
            }
            endpoint.RoutingHeaders.Add("x-match", "any");
            endpoint.Declare();
            var pipeLine = new RabbitMqPipeline(definition.PipelineName, session, RabbitMqPipeline.PipelineType.Headers);
            pipeLine.Declare();
            pipeLine.Bind(endpoint);
            return endpoint;
        }

        public IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes)
        {
            return endpointNameConvention.GetEndpointDefinitions(handlerTypes);
        }
    }
}
