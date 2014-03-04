using NMSD.Cronus.Messaging;
using NMSD.Cronus.Transports;

namespace NMSD.Cronus.Pipelining
{
    public interface IConsumer : ITransportIMessage
    {
        bool Consume(IEndpoint endpoint);
    }
}