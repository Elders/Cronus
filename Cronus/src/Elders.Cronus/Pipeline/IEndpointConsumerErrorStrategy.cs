using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpointConsumerErrorStrategy
    {
        void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition);
        bool Handle(ErrorMessage errorMessage);
    }

    public interface IEndpointConsumerSuccessStrategy
    {
        void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition);
        bool Handle(IMessage successMessage);
    }

    public interface IEndpointConsumerRetryStrategy
    {
        void Initialize(IEndpointFactory endpointFactory, EndpointDefinition endpointDefinition);
        bool Handle(IMessage retryMessage);
    }
}