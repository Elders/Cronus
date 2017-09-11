using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IConsumerSettings : ISettingsBuilder
    {
    }

    public interface IConsumerSettings<TContract> : IConsumerSettings where TContract : IMessage { }
}
