using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IConsumerSettings : ISettingsBuilder
    {
        int NumberOfWorkers { get; set; }

        MessageThreshold MessageTreshold { get; set; }
    }

    public interface IConsumerSettings<TContract> : IConsumerSettings where TContract : IMessage { }
}
