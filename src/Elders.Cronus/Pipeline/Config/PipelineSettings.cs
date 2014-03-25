namespace Elders.Cronus.Pipeline.Config
{
    public class PipelineSettings
    {
        public IPipelineNameConvention PipelineNameConvention { get; set; }

        public IEndpointNameConvention EndpointNameConvention { get; set; }
    }
}
