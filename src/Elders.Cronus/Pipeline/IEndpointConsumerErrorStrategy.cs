using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpointConsumerErrorStrategy
    {
        void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition);
        bool Handle(TransportMessage errorMessage);
    }

    public interface IEndpointConsumerSuccessStrategy
    {
        void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition);
        bool Handle(TransportMessage successMessage);
    }

    public interface IEndpointConsumerRetryStrategy
    {
        void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition);
        bool Handle(TransportMessage retryMessage);
    }
}