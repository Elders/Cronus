using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IConsumerSettings : ISettingsBuilder
    {
        int NumberOfWorkers { get; set; }
        MessageThreshold MessageTreshold { get; set; }
    }

    public interface ICanConfigureCircuitBreaker : ISettingsBuilder
    {

    }

    public interface IConsumerSettings<TContract> : IConsumerSettings, ICanConfigureCircuitBreaker
        where TContract : IMessage
    {

    }
}