using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipelining.Transport.Strategy;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public static class CommandTransportStrategyConfig
    {
        public static IPipelineTransportSettings<IEndpointConsumer<ICommand>> UsePipelinePerApplication(this IPipelineTransportSettings<IEndpointConsumer<ICommand>> transport)
        {
            transport.PipelineSettings.PipelineNameConvention = new CommandPipelinePerApplication();
            return transport;
        }

        public static IPipelineTransportSettings<IEndpointConsumer<ICommand>> UseEndpointPerBoundedContext(this IPipelineTransportSettings<IEndpointConsumer<ICommand>> transport)
        {
            transport.PipelineSettings.EndpointNameConvention = new CommandHandlerEndpointPerBoundedContext(transport.PipelineSettings.PipelineNameConvention);
            return transport;
        }

    }
}