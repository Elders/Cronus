using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Hosting;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Transports.Conventions;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public static class CommandConsumerTransportConfig
    {
        public static IPipelineTransportSettings<IStartableConsumer<ICommand>> UsePipelinePerApplication(this IPipelineTransportSettings<IStartableConsumer<ICommand>> transport)
        {
            transport.PipelineSettings.PipelineNameConvention = new CommandPipelinePerApplication();
            return transport;
        }

        public static IPipelineTransportSettings<IStartableConsumer<ICommand>> UseEndpointPerBoundedContext(this IPipelineTransportSettings<IStartableConsumer<ICommand>> transport)
        {
            transport.PipelineSettings.EndpointNameConvention = new CommandHandlerEndpointPerBoundedContext(transport.PipelineSettings.PipelineNameConvention);
            return transport;
        }

        public static IPipelineTransportSettings<IPublisher<ICommand>> UsePipelinePerApplication(this IPipelineTransportSettings<IPublisher<ICommand>> transport)
        {
            transport.PipelineSettings.PipelineNameConvention = new CommandPipelinePerApplication();
            return transport;
        }
    }
}