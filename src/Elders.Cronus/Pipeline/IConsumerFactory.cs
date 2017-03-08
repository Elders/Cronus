namespace Elders.Cronus.Pipeline
{
    public interface IConsumerFactory
    {
        ContinuousConsumer CreateConsumer();
    }
}
