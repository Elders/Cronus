using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Transports.Conventions;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public static class CommandTransportConfig
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