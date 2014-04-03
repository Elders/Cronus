using System;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline
{
    public class ErrorEndpointPerEndpoint : IEndpointConsumerErrorStrategy
    {
        string endpointWhereErrorOccured;

        private string errorPipelineName;

        private readonly IPipelineFactory<IPipeline> pipelineFactory;

        private readonly ProtoregSerializer serializer;

        public ErrorEndpointPerEndpoint(IPipelineFactory<IPipeline> pipelineFactory, ProtoregSerializer serializer)
        {
            this.serializer = serializer;
            this.pipelineFactory = pipelineFactory;
        }

        public bool Handle(ErrorMessage errorMessage)
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

    public class EventStorePublishEventsOnSuccessPersist : IEndpointConsumerSuccessStrategy
    {

        private readonly IPublisher<IEvent> eventPublisher;
        public EventStorePublishEventsOnSuccessPersist(IPublisher<IEvent> eventPublisher)
        {
            this.eventPublisher = eventPublisher;
        }

        public void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition) { }

        public bool Handle(IMessage successMessage)
        {
            DomainMessageCommit successCommit = successMessage as DomainMessageCommit;
            if (successCommit != null)
            {
                successCommit.Events.ForEach(e => eventPublisher.Publish(e));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}