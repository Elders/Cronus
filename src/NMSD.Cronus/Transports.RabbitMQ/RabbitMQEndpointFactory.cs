using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.RabbitMQ;

namespace NMSD.Cronus.Transports.RabbitMQ
{
    public class RabbitMqEndpointFactory : IEndpointFactory
    {
        private RabbitMqPipelineFactory pipeFactory;
        private RabbitMqSession session;
        public RabbitMqEndpointFactory(RabbitMqSession session)
        {
            this.session = session;
            pipeFactory = new RabbitMqPipelineFactory(session);
        }

        public void BuildEndpoint(EndpointDefinition definition)
        {
            var endpoint = new RabbitMqEndpoint(definition.EndpointName, session);
            endpoint.Declare();
            var pipeLine = pipeFactory.GetPipeline(definition.PipelineName);
            pipeLine.Declare();
            pipeLine.AttachEndpoint(endpoint);
            pipeLine.Close();
            endpoint.Close();
        }
        public RabbitMqEndpoint CreateEndpoint(EndpointDefinition definition)
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
            pipeLine.AttachEndpoint(endpoint);
            return endpoint;
        }
        IEndpoint IEndpointFactory.CreateEndpoint(EndpointDefinition definition)
        {
            return CreateEndpoint(definition);
        }
    }
}
