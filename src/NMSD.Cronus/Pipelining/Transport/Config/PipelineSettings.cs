using NMSD.Cronus.Transports;
using NMSD.Cronus.Transports.Conventions;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public class PipelineSettings
    {
        public IPipelineNameConvention PipelineNameConvention { get; set; }

        public IEndpointNameConvention EndpointNameConvention { get; set; }
    }
}
