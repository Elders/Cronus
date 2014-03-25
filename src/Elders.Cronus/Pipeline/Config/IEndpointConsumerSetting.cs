namespace Elders.Cronus.Pipeline.Config
{
    public interface IEndpointConsumerSetting : IConsumerSettings<PipelineTransportSettings>
    {
        PipelineSettings PipelineSettings { get; set; }
    }
}