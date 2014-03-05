using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipelining.Transport.Strategy;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public static class EventStoreTransportStrategyConfig
    {
        public static IPipelineTransportSettings<IPublisher<ICommand>> UsePipelinePerApplication(this IPipelineTransportSettings<IPublisher<ICommand>> transport)
        {
            transport.PipelineSettings.PipelineNameConvention = new CommandPipelinePerApplication();
            return transport;
        }

        public static IPipelineTransportSettings<IPublisher<IEvent>> UsePipelinePerApplication(this IPipelineTransportSettings<IPublisher<IEvent>> transport)
        {
            transport.PipelineSettings.PipelineNameConvention = new EventPipelinePerApplication();
            return transport;
        }
    }
}