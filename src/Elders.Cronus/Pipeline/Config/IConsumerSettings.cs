using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IConsumerSettings
    {
        int NumberOfWorkers { get; set; }
        MessageThreshold MessageTreshold { get; set; }
    }

    public interface ICanConfigureCircuitBreaker : ISettingsBuilder
    {

    }

    public interface IConsumerSettings<TContract> : ISettingsBuilder, IConsumerSettings, IPipelinePublisherSettings<TContract>, ICanConfigureCircuitBreaker
        where TContract : IMessage
    {

    }
}