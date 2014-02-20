namespace NMSD.Cronus.Transports
{
    public interface IEndpointFactory
    {
        IEndpoint CreateEndpoint(EndpointDefinition definition);
    }
}