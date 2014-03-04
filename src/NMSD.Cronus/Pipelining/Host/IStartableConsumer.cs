using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipelining;

namespace NMSD.Cronus.Hosting
{
    public interface IStartableConsumer<out TMessage> : IConsumer
            where TMessage : IMessage
    {
        void Start(int numberOfWorkers);

        void Stop();
    }
}