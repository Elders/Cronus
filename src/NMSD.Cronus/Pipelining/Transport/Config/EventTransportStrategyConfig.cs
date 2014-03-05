using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipelining.Transport.Strategy;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public static class EventTransportStrategyConfig
    {
        public static IPipelineTransportSettings<IEndpointConsumer<IEvent>> UsePipelinePerApplication(this IPipelineTransportSettings<IEndpointConsumer<IEvent>> transport)
        {
            transport.PipelineSettings.PipelineNameConvention = new EventPipelinePerApplication();
            return transport;
        }

        public static IPipelineTransportSettings<IEndpointConsumer<IEvent>> UseEndpointPerBoundedContext(this IPipelineTransportSettings<IEndpointConsumer<IEvent>> transport)
        {
            transport.PipelineSettings.EndpointNameConvention = new EventHandlerEndpointPerBoundedContext(transport.PipelineSettings.PipelineNameConvention);
            return transport;
        }

        public static IPipelineTransportSettings<IEndpointConsumer<IEvent>> UseEndpointPerHandler(this IPipelineTransportSettings<IEndpointConsumer<IEvent>> transport)
        {
            transport.PipelineSettings.EndpointNameConvention = new EventHandlerEndpointPerHandler(transport.PipelineSettings.PipelineNameConvention);
            return transport;
        }
    }
}