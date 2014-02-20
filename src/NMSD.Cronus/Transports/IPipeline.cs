namespace NMSD.Cronus.Transports
{
    public interface IPipeline
    {
        void Push(EndpointMessage message);
    }
}