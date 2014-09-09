using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpointPostConsume
    {
        IEndpointConsumerSuccessStrategy SuccessStrategy { get; set; }

        IEndpointConsumerRetryStrategy RetryStrategy { get; set; }

        IEndpointConsumerErrorStrategy ErrorStrategy { get; set; }
    }

    public class NoEndpointPostConsume : IEndpointPostConsume
    {
        public NoEndpointPostConsume()
        {
            this.ErrorStrategy = new EndpointPostConsumeStrategy.NoErrorStrategy();
            this.RetryStrategy = new EndpointPostConsumeStrategy.NoRetryStrategy();
            this.SuccessStrategy = new EndpointPostConsumeStrategy.NoSuccessStrategy();
        }

        public IEndpointConsumerErrorStrategy ErrorStrategy { get; set; }

        public IEndpointConsumerRetryStrategy RetryStrategy { get; set; }

        public IEndpointConsumerSuccessStrategy SuccessStrategy { get; set; }
    }

    public class DefaultEndpointPostConsume : IEndpointPostConsume
    {
        public DefaultEndpointPostConsume(IPipelineFactory<IPipeline> pipelineFactory, ISerializer serializer)
        {
            this.ErrorStrategy = new EndpointPostConsumeStrategy.ErrorEndpointPerEndpoint(pipelineFactory, serializer);
            this.RetryStrategy = new EndpointPostConsumeStrategy.RetryEndpointPerEndpoint(pipelineFactory, serializer);
            this.SuccessStrategy = new EndpointPostConsumeStrategy.NoSuccessStrategy();
        }

        public IEndpointConsumerErrorStrategy ErrorStrategy { get; set; }

        public IEndpointConsumerRetryStrategy RetryStrategy { get; set; }

        public IEndpointConsumerSuccessStrategy SuccessStrategy { get; set; }
    }

    public class EndpointPostConsumeStrategy
    {
        public class RetryEndpointPerEndpoint : IEndpointConsumerRetryStrategy
        {
            string endpointWhereErrorOccured;

            private string retryPipelineName;

            private readonly IPipelineFactory<IPipeline> pipelineFactory;

            private readonly ISerializer serializer;

            public RetryEndpointPerEndpoint(IPipelineFactory<IPipeline> pipelineFactory, ISerializer serializer)
            {
                this.serializer = serializer;
                this.pipelineFactory = pipelineFactory;
            }

            public bool Handle(TransportMessage message)
            {
                pipelineFactory
                    .GetPipeline(retryPipelineName)
                    .Push(message.AsEndpointMessage(serializer, endpointWhereErrorOccured));
                return true;
            }

            public void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition)
            {
                endpointWhereErrorOccured = endpointDefinition.EndpointName;
                retryPipelineName = endpointDefinition.PipelineName + ".RetryScheduler";

                Dictionary<string, object> headers = new Dictionary<string, object>();
                headers.Add("x-dead-letter-exchange", endpointDefinition.PipelineName);
                headers.Add("x-message-ttl", 500);

                EndpointDefinition retryEndpoint = new EndpointDefinition(retryPipelineName, endpointWhereErrorOccured + ".Retry", headers, endpointWhereErrorOccured);
                endpointFactory.CreateTopicEndpoint(retryEndpoint);
            }

        }

        public class ErrorEndpointPerEndpoint : IEndpointConsumerErrorStrategy
        {
            string endpointWhereErrorOccured;

            private string errorPipelineName;

            private readonly IPipelineFactory<IPipeline> pipelineFactory;

            private readonly ISerializer serializer;

            public ErrorEndpointPerEndpoint(IPipelineFactory<IPipeline> pipelineFactory, ISerializer serializer)
            {
                this.serializer = serializer;
                this.pipelineFactory = pipelineFactory;
            }

            public bool Handle(TransportMessage errorMessage)
            {
                pipelineFactory
                    .GetPipeline(errorPipelineName)
                    .Push(errorMessage.AsEndpointMessage(serializer, endpointWhereErrorOccured));
                return true;
            }

            public void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition)
            {
                endpointWhereErrorOccured = endpointDefinition.EndpointName;
                errorPipelineName = endpointDefinition.PipelineName + ".Errors";
                EndpointDefinition errorEndpoint = new EndpointDefinition(errorPipelineName, endpointWhereErrorOccured + ".Errors", null, endpointWhereErrorOccured);
                endpointFactory.CreateTopicEndpoint(errorEndpoint);
            }

        }

        public class NoSuccessStrategy : IEndpointConsumerSuccessStrategy
        {
            public bool Handle(TransportMessage successMessage) { return true; }

            public void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition) { }
        }

        public class NoErrorStrategy : IEndpointConsumerErrorStrategy
        {
            public bool Handle(TransportMessage errorMessage) { return true; }

            public void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition) { }
        }

        public class NoRetryStrategy : IEndpointConsumerRetryStrategy
        {
            public bool Handle(TransportMessage errorMessage) { return true; }

            public void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition) { }
        }

    }
}