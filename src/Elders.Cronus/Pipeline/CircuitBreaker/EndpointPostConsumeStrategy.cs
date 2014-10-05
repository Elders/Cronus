using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline.CircuitBreaker
{
    public class EndpointPostConsumeStrategy
    {
        public class RetryEndpointPerEndpoint : ICircuitBreakerRetryStrategy
        {
            string endpointWhereErrorOccured;

            private string retryPipelineName;

            private readonly IPipelineTransport transport;

            private readonly ISerializer serializer;

            public RetryEndpointPerEndpoint(IPipelineTransport transport, ISerializer serializer, EndpointDefinition endpointDefinition)
            {
                this.serializer = serializer;
                this.transport = transport;

                endpointWhereErrorOccured = endpointDefinition.EndpointName;
                retryPipelineName = endpointDefinition.PipelineName + ".RetryScheduler";
                Dictionary<string, object> headers = new Dictionary<string, object>();


                EndpointDefinition retryEndpoint = new EndpointDefinition(retryPipelineName, endpointWhereErrorOccured + ".Retry", headers, endpointWhereErrorOccured);
                transport.EndpointFactory.CreateEndpoint(retryEndpoint);
            }

            public bool Handle(TransportMessage message)
            {
                transport.PipelineFactory
                    .GetPipeline(retryPipelineName)
                    .Push(message.AsEndpointMessage(serializer, endpointWhereErrorOccured));
                return true;
            }
        }

        public class ErrorEndpointPerEndpoint : ICircuitBreakerErrorStrategy
        {
            string endpointWhereErrorOccured;

            private string errorPipelineName;

            private readonly IPipelineTransport transport;

            private readonly ISerializer serializer;

            public ErrorEndpointPerEndpoint(IPipelineTransport transport, ISerializer serializer, EndpointDefinition endpointDefinition)
            {
                this.serializer = serializer;
                this.transport = transport;
                endpointWhereErrorOccured = endpointDefinition.EndpointName;
                errorPipelineName = endpointDefinition.PipelineName + ".Errors";
                EndpointDefinition errorEndpoint = new EndpointDefinition(errorPipelineName, endpointWhereErrorOccured + ".Errors", null, endpointWhereErrorOccured);
                transport.EndpointFactory.CreateTopicEndpoint(errorEndpoint);
            }

            public bool Handle(TransportMessage errorMessage)
            {
                transport.PipelineFactory
                      .GetPipeline(errorPipelineName)
                      .Push(errorMessage.AsEndpointMessage(serializer, endpointWhereErrorOccured));
                return true;
            }
        }

        public class NoSuccessStrategy : ICircuitBreakerSuccessStrategy
        {
            public bool Handle(TransportMessage successMessage) { return true; }

            public void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition) { }
        }

        public class NoErrorStrategy : ICircuitBreakerErrorStrategy
        {
            public bool Handle(TransportMessage errorMessage) { return true; }

            public void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition) { }
        }

        public class NoRetryStrategy : ICircuitBreakerRetryStrategy
        {
            public bool Handle(TransportMessage errorMessage) { return true; }

            public void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition) { }
        }

    }
}
