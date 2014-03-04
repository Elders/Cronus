using System.Reflection;
using NMSD.Cronus.Messaging;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public class PipelinePublisherSettings<T> where T : IPublisher
    {
        public IPipelineTransportSettings<T> Transport;
        public Assembly MessagesAssembly { get; set; }
    }
}
