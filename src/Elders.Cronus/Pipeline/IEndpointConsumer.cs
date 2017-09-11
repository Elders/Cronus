namespace Elders.Cronus.Pipeline
{
    public interface IConsumer
    {

    }

    interface IInMemoryConsumer : IConsumer
    {

    }

    public interface IEndpointConsumer : IConsumer
    {
        string Name { get; }

        void Start();

        void Stop();
    }
}
