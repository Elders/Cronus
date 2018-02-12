namespace Elders.Cronus.Pipeline.Config
{
    public interface IConsumerSettings : ISettingsBuilder
    {
        int NumberOfWorkers { get; set; }

    }

    public interface IConsumerSettings<TContract> : IConsumerSettings where TContract : IMessage { }
}
