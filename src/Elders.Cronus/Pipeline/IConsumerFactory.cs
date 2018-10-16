namespace Elders.Cronus.Pipeline
{
    public interface IConsumerFactory<T>
    {
        ContinuousConsumer<T> CreateConsumer();
    }
}
